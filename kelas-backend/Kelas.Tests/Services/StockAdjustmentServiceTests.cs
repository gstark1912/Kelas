using Kelas.Domain.Entities;
using Kelas.Domain.Exceptions;
using Kelas.Domain.Interfaces.Repositories;
using Kelas.Domain.Models.Requests;
using Kelas.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Kelas.Tests.Services;

public class StockAdjustmentServiceTests
{
    private readonly Mock<IMongoClient> _clientMock = new();
    private readonly Mock<IClientSessionHandle> _sessionMock = new();
    private readonly Mock<IRawMaterialRepository> _rawMaterialRepositoryMock = new();
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<IStockMovementRepository> _stockMovementRepositoryMock = new();
    private readonly Mock<IStockRepository> _stockRepositoryMock = new();
    private readonly StockAdjustmentService _service;

    public StockAdjustmentServiceTests()
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

        _stockMovementRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<StockMovement>(), It.IsAny<object?>()))
            .ReturnsAsync((StockMovement movement, object? _) =>
            {
                movement.Id = ObjectId.GenerateNewId();
                return movement;
            });
        _stockRepositoryMock
            .Setup(x => x.UpsertIncrementQuantityAsync(
                It.IsAny<string>(),
                It.IsAny<ObjectId>(),
                It.IsAny<decimal>(),
                It.IsAny<object?>()))
            .Returns(Task.CompletedTask);

        _service = new StockAdjustmentService(
            _clientMock.Object,
            _rawMaterialRepositoryMock.Object,
            _productRepositoryMock.Object,
            _stockMovementRepositoryMock.Object,
            _stockRepositoryMock.Object);
    }

    [Theory]
    [InlineData("RawMaterial", 10, 15, "AdjustmentIncrease", 5)]
    [InlineData("RawMaterial", 10, 4, "AdjustmentDecrease", -6)]
    [InlineData("FinishedProduct", 3, 8, "AdjustmentIncrease", 5)]
    [InlineData("FinishedProduct", 9, 2, "AdjustmentDecrease", -7)]
    public async Task CreateAsync_WithSupportedItemType_CreatesMovementAndUpdatesStock(
        string itemType,
        decimal currentStock,
        decimal newStock,
        string expectedMovementType,
        decimal expectedDelta)
    {
        var itemId = ObjectId.GenerateNewId();
        SetupItem(itemType, itemId, "Item Test");
        _stockRepositoryMock
            .Setup(x => x.GetByItemAsync(itemType, itemId.ToString()))
            .ReturnsAsync(new Stock
            {
                ItemType = itemType,
                ItemId = itemId,
                CurrentQuantity = currentStock
            });

        var result = await _service.CreateAsync(new CreateStockAdjustmentRequest
        {
            ItemType = itemType,
            ItemId = itemId.ToString(),
            NewStock = newStock,
            Reason = "Corrección de inventario",
            Date = new DateTime(2026, 5, 13, 0, 0, 0, DateTimeKind.Utc),
            Notes = "Ajuste test"
        });

        Assert.Equal(itemType, result.ItemType);
        Assert.Equal(itemId.ToString(), result.ItemId);
        Assert.Equal("Item Test", result.ItemName);
        Assert.Equal(expectedMovementType, result.MovementType);
        Assert.Equal(expectedDelta, result.Quantity);
        Assert.Equal(newStock, result.NewStock);

        _stockMovementRepositoryMock.Verify(x => x.CreateAsync(
            It.Is<StockMovement>(movement =>
                movement.ItemType == itemType &&
                movement.ItemId == itemId &&
                movement.MovementType == expectedMovementType &&
                movement.Quantity == expectedDelta &&
                movement.AdjustmentReason == "Corrección de inventario" &&
                movement.Notes == "Ajuste test"),
            It.IsAny<object?>()), Times.Once);
        _stockRepositoryMock.Verify(x => x.UpsertIncrementQuantityAsync(
            itemType,
            itemId,
            expectedDelta,
            It.IsAny<object?>()), Times.Once);
        _sessionMock.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidItemType_ThrowsBusinessException()
    {
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _service.CreateAsync(new CreateStockAdjustmentRequest
        {
            ItemType = "Other",
            ItemId = ObjectId.GenerateNewId().ToString(),
            NewStock = 1,
            Reason = "Otro",
            Date = DateTime.UtcNow
        }));

        Assert.Equal("El tipo de ítem debe ser RawMaterial o FinishedProduct.", exception.Message);
        _stockMovementRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<StockMovement>(), It.IsAny<object?>()), Times.Never);
    }

    [Theory]
    [InlineData("RawMaterial", "RawMaterial")]
    [InlineData("FinishedProduct", "Product")]
    public async Task CreateAsync_WithMissingItem_ThrowsNotFoundException(string itemType, string expectedEntity)
    {
        var itemId = ObjectId.GenerateNewId().ToString();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAsync(new CreateStockAdjustmentRequest
        {
            ItemType = itemType,
            ItemId = itemId,
            NewStock = 1,
            Reason = "Otro",
            Date = DateTime.UtcNow
        }));

        Assert.Equal($"{expectedEntity} con id '{itemId}' no encontrado.", exception.Message);
        _stockMovementRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<StockMovement>(), It.IsAny<object?>()), Times.Never);
    }

    private void SetupItem(string itemType, ObjectId itemId, string name)
    {
        if (itemType == "RawMaterial")
        {
            _rawMaterialRepositoryMock
                .Setup(x => x.GetByIdAsync(itemId.ToString()))
                .ReturnsAsync(new RawMaterial
                {
                    Id = itemId,
                    Name = name,
                    Unit = "gr",
                    IsActive = true
                });
            return;
        }

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(itemId.ToString()))
            .ReturnsAsync(new Product
            {
                Id = itemId,
                Name = name,
                IsVisible = true
            });
    }
}
