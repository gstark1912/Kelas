using Kelas.Domain.Entities;
using Kelas.Domain.Exceptions;
using Kelas.Domain.Interfaces.Repositories;
using Kelas.Domain.Interfaces.Services;
using Kelas.Domain.Models.Requests;
using Kelas.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Kelas.Tests.Services;

public class ProductionServiceTests
{
    private readonly Mock<IMongoClient> _clientMock = new();
    private readonly Mock<IClientSessionHandle> _sessionMock = new();
    private readonly Mock<IProductionBatchRepository> _productionBatchRepositoryMock = new();
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<IRawMaterialRepository> _rawMaterialRepositoryMock = new();
    private readonly Mock<IStockAdjustmentService> _stockAdjustmentServiceMock = new();
    private readonly ProductionService _service;

    public ProductionServiceTests()
    {
        _clientMock
            .Setup(x => x.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_sessionMock.Object);
        _sessionMock
            .Setup(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _sessionMock
            .Setup(x => x.AbortTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _service = new ProductionService(
            _clientMock.Object,
            _productionBatchRepositoryMock.Object,
            _productRepositoryMock.Object,
            _rawMaterialRepositoryMock.Object,
            _stockAdjustmentServiceMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithQuantityZero_ThrowsBusinessException()
    {
        var request = new CreateProductionBatchRequest
        {
            ProductId = ObjectId.GenerateNewId().ToString(),
            Quantity = 0,
            Date = DateTime.UtcNow
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(() => _service.CreateAsync(request));

        Assert.Equal("La cantidad debe ser mayor a 0.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_WithProductWithoutRecipe_ThrowsBusinessException()
    {
        var product = new Product
        {
            Id = ObjectId.GenerateNewId(),
            Name = "Vela",
            IsVisible = true,
            Recipe = new List<RecipeItem>()
        };
        _productRepositoryMock.Setup(x => x.GetByIdAsync(product.Id.ToString())).ReturnsAsync(product);

        var request = new CreateProductionBatchRequest
        {
            ProductId = product.Id.ToString(),
            Quantity = 2,
            Date = DateTime.UtcNow
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(() => _service.CreateAsync(request));

        Assert.Equal("El producto debe tener una receta definida.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_WithInsufficientStockWithoutConfirmation_ThrowsStructuredExceptionAndDoesNotWrite()
    {
        var rawMaterialId = ObjectId.GenerateNewId();
        var product = CreateProduct(rawMaterialId, recipeQuantity: 3);
        var rawMaterial = CreateRawMaterial(rawMaterialId, "Cera", lastPricePerUnit: 10);
        var stock = new Stock { ItemType = "RawMaterial", ItemId = rawMaterialId, CurrentQuantity = 2 };

        SetupRecipeData(product, rawMaterial, stock);

        var request = new CreateProductionBatchRequest
        {
            ProductId = product.Id.ToString(),
            Quantity = 2,
            Date = DateTime.UtcNow,
            ConfirmInsufficientStock = false
        };

        var exception = await Assert.ThrowsAsync<InsufficientStockException>(() => _service.CreateAsync(request));

        Assert.Single(exception.Items);
        Assert.Equal("Cera", exception.Items[0].RawMaterialName);
        Assert.Equal(6, exception.Items[0].RequiredQuantity);
        Assert.Equal(2, exception.Items[0].AvailableQuantity);
        Assert.Equal(4, exception.Items[0].MissingQuantity);
        _productionBatchRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<ProductionBatch>(), It.IsAny<object?>()), Times.Never);
        _stockAdjustmentServiceMock.Verify(x => x.RegisterMovementAsync(
            It.IsAny<string>(),
            It.IsAny<ObjectId>(),
            It.IsAny<string>(),
            It.IsAny<decimal>(),
            It.IsAny<DateTime>(),
            It.IsAny<string?>(),
            It.IsAny<ObjectId?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<object?>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_CreatesBatchMovementsAndStockUpdates()
    {
        var rawMaterialId = ObjectId.GenerateNewId();
        var product = CreateProduct(rawMaterialId, recipeQuantity: 2);
        var rawMaterial = CreateRawMaterial(rawMaterialId, "Cera", lastPricePerUnit: 5);
        var stock = new Stock { ItemType = "RawMaterial", ItemId = rawMaterialId, CurrentQuantity = 20 };

        SetupRecipeData(product, rawMaterial, stock);

        _productionBatchRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<ProductionBatch>(), It.IsAny<object?>()))
            .ReturnsAsync((ProductionBatch batch, object? _) =>
            {
                batch.Id = ObjectId.GenerateNewId();
                batch.CreatedAt = DateTime.UtcNow;
                return batch;
            });
        _stockAdjustmentServiceMock
            .Setup(x => x.RegisterMovementAsync(
                It.IsAny<string>(),
                It.IsAny<ObjectId>(),
                It.IsAny<string>(),
                It.IsAny<decimal>(),
                It.IsAny<DateTime>(),
                It.IsAny<string?>(),
                It.IsAny<ObjectId?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<object?>()))
            .ReturnsAsync((string itemType, ObjectId itemId, string movementType, decimal quantity, DateTime date, string? referenceType, ObjectId? referenceId, string? adjustmentReason, string? notes, object? _) => new StockMovement
            {
                ItemType = itemType,
                ItemId = itemId,
                MovementType = movementType,
                Quantity = quantity,
                Date = date,
                ReferenceType = referenceType,
                ReferenceId = referenceId,
                AdjustmentReason = adjustmentReason,
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            });

        var request = new CreateProductionBatchRequest
        {
            ProductId = product.Id.ToString(),
            Quantity = 3,
            Date = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            Notes = "Tanda QA"
        };

        var result = await _service.CreateAsync(request);

        Assert.Equal(product.Id.ToString(), result.ProductId);
        Assert.Equal(3, result.Quantity);
        Assert.Equal(30, result.TotalCost);
        Assert.Equal(10, result.UnitCost);
        Assert.Single(result.Ingredients);
        Assert.Equal(6, result.Ingredients[0].QuantityUsed);
        Assert.Equal(5, result.Ingredients[0].PricePerUnit);
        Assert.Equal(30, result.Ingredients[0].Cost);

        _stockAdjustmentServiceMock.Verify(x => x.RegisterMovementAsync(
            "RawMaterial",
            rawMaterialId,
            "ProductionConsumption",
            -6,
            It.IsAny<DateTime>(),
            "Production",
            It.IsAny<ObjectId?>(),
            null,
            "Tanda QA",
            It.IsAny<object?>()), Times.Once);
        _stockAdjustmentServiceMock.Verify(x => x.RegisterMovementAsync(
            "FinishedProduct",
            product.Id,
            "ProductionOutput",
            3,
            It.IsAny<DateTime>(),
            "Production",
            It.IsAny<ObjectId?>(),
            null,
            "Tanda QA",
            It.IsAny<object?>()), Times.Once);
        _sessionMock.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_ReturnsFilteredItemsAndKpis()
    {
        var productId = ObjectId.GenerateNewId();
        var product = new Product { Id = productId, Name = "Vela", IsVisible = true };
        var batches = new List<ProductionBatch>
        {
            new() { Id = ObjectId.GenerateNewId(), ProductId = productId, Quantity = 2, TotalCost = 20, UnitCost = 10, Date = DateTime.UtcNow },
            new() { Id = ObjectId.GenerateNewId(), ProductId = productId, Quantity = 3, TotalCost = 45, UnitCost = 15, Date = DateTime.UtcNow }
        };

        _productionBatchRepositoryMock
            .Setup(x => x.GetAsync(productId.ToString(), null, null))
            .ReturnsAsync(batches);
        _productRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.Is<IEnumerable<ObjectId>>(ids => ids.Contains(productId))))
            .ReturnsAsync(new List<Product> { product });

        var result = await _service.GetAsync(productId.ToString());

        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item => Assert.Equal("Vela", item.ProductName));
        Assert.Equal(5, result.Kpis.TotalUnitsProduced);
        Assert.Equal(65, result.Kpis.TotalCost);
    }

    private void SetupRecipeData(Product product, RawMaterial rawMaterial, Stock stock)
    {
        _productRepositoryMock.Setup(x => x.GetByIdAsync(product.Id.ToString())).ReturnsAsync(product);
        _rawMaterialRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<ObjectId>>()))
            .ReturnsAsync(new List<RawMaterial> { rawMaterial });
        _stockAdjustmentServiceMock
            .Setup(x => x.GetStockByItemsAsync("RawMaterial", It.IsAny<IEnumerable<ObjectId>>()))
            .ReturnsAsync(new List<Stock> { stock });
    }

    private static Product CreateProduct(ObjectId rawMaterialId, decimal recipeQuantity) => new()
    {
        Id = ObjectId.GenerateNewId(),
        Name = "Vela",
        IsVisible = true,
        Recipe = new List<RecipeItem>
        {
            new() { RawMaterialId = rawMaterialId, Quantity = recipeQuantity }
        }
    };

    private static RawMaterial CreateRawMaterial(ObjectId id, string name, decimal lastPricePerUnit) => new()
    {
        Id = id,
        Name = name,
        Unit = "gr",
        LastPricePerUnit = lastPricePerUnit,
        IsActive = true
    };
}
