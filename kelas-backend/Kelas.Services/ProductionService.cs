using Kelas.Domain.Entities;
using Kelas.Domain.Exceptions;
using Kelas.Domain.Interfaces.Repositories;
using Kelas.Domain.Interfaces.Services;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kelas.Services;

public class ProductionService : IProductionService
{
    private readonly IMongoClient _client;
    private readonly IProductionBatchRepository _productionBatchRepository;
    private readonly IProductRepository _productRepository;
    private readonly IRawMaterialRepository _rawMaterialRepository;
    private readonly IStockAdjustmentService _stockAdjustmentService;

    public ProductionService(
        IMongoClient client,
        IProductionBatchRepository productionBatchRepository,
        IProductRepository productRepository,
        IRawMaterialRepository rawMaterialRepository,
        IStockAdjustmentService stockAdjustmentService)
    {
        _client = client;
        _productionBatchRepository = productionBatchRepository;
        _productRepository = productRepository;
        _rawMaterialRepository = rawMaterialRepository;
        _stockAdjustmentService = stockAdjustmentService;
    }

    public async Task<ProductionBatchDetailResponse> CreateAsync(CreateProductionBatchRequest request)
    {
        ValidateCreateRequest(request);

        if (!ObjectId.TryParse(request.ProductId, out _))
            throw new BusinessException("El identificador de producto no es válido.");

        var product = await _productRepository.GetByIdAsync(request.ProductId)
            ?? throw new NotFoundException("Product", request.ProductId);

        if (!product.IsVisible)
            throw new BusinessException("El producto no está disponible para producción.");

        if (product.Recipe.Count == 0)
            throw new BusinessException("El producto debe tener una receta definida.");

        var rawMaterialIds = product.Recipe.Select(x => x.RawMaterialId).Distinct().ToList();
        var rawMaterials = await _rawMaterialRepository.GetByIdsAsync(rawMaterialIds);

        if (rawMaterials.Count != rawMaterialIds.Count)
        {
            var foundIds = rawMaterials.Select(x => x.Id).ToHashSet();
            var missing = rawMaterialIds.First(x => !foundIds.Contains(x));
            throw new BusinessException($"La materia prima con id '{missing}' no fue encontrada.");
        }

        var rawMaterialsById = rawMaterials.ToDictionary(x => x.Id);
        var stocks = await _stockAdjustmentService.GetStockByItemsAsync("RawMaterial", rawMaterialIds);
        var stocksByItemId = stocks.ToDictionary(x => x.ItemId);
        var ingredientSnapshots = BuildIngredientSnapshots(product, request.Quantity, rawMaterialsById);
        var insufficientItems = BuildInsufficientStockItems(ingredientSnapshots, rawMaterialsById, stocksByItemId);

        if (insufficientItems.Count > 0 && !request.ConfirmInsufficientStock)
            throw new InsufficientStockException(insufficientItems);

        var totalCost = ingredientSnapshots.Sum(x => x.Cost);
        var batch = new ProductionBatch
        {
            ProductId = product.Id,
            Quantity = request.Quantity,
            Date = request.Date!.Value,
            TotalCost = totalCost,
            UnitCost = totalCost / request.Quantity,
            Ingredients = ingredientSnapshots,
            Notes = request.Notes?.Trim()
        };

        using var session = await _client.StartSessionAsync();
        session.StartTransaction();
        try
        {
            var created = await _productionBatchRepository.CreateAsync(batch, session);

            foreach (var ingredient in created.Ingredients)
            {
                await _stockAdjustmentService.RegisterMovementAsync(
                    "RawMaterial",
                    ingredient.RawMaterialId,
                    "ProductionConsumption",
                    -ingredient.QuantityUsed,
                    created.Date,
                    referenceType: "Production",
                    referenceId: created.Id,
                    notes: created.Notes,
                    session: session);
            }

            await _stockAdjustmentService.RegisterMovementAsync(
                "FinishedProduct",
                product.Id,
                "ProductionOutput",
                request.Quantity,
                created.Date,
                referenceType: "Production",
                referenceId: created.Id,
                notes: created.Notes,
                session: session);

            await session.CommitTransactionAsync();

            return MapToDetailResponse(created, product, rawMaterialsById);
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    public async Task<ProductionBatchListResultResponse> GetAsync(string? productId = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        if (!string.IsNullOrWhiteSpace(productId) && !ObjectId.TryParse(productId, out _))
            throw new BusinessException("El identificador de producto no es válido.");

        var batches = await _productionBatchRepository.GetAsync(productId, dateFrom, dateTo);
        var productsById = await LoadProductsByIdAsync(batches.Select(x => x.ProductId));

        var items = batches.Select(batch =>
        {
            productsById.TryGetValue(batch.ProductId, out var product);
            return MapToListResponse(batch, product);
        }).ToList();

        return new ProductionBatchListResultResponse
        {
            Items = items,
            Kpis = new ProductionKpisResponse
            {
                TotalUnitsProduced = batches.Sum(x => x.Quantity),
                TotalCost = batches.Sum(x => x.TotalCost)
            }
        };
    }

    public async Task<ProductionBatchDetailResponse> GetByIdAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            throw new BusinessException("El identificador de producción no es válido.");

        var batch = await _productionBatchRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("ProductionBatch", id);

        var product = await _productRepository.GetByIdAsync(batch.ProductId.ToString());
        var rawMaterialIds = batch.Ingredients.Select(x => x.RawMaterialId).Distinct().ToList();
        var rawMaterials = rawMaterialIds.Any()
            ? await _rawMaterialRepository.GetByIdsAsync(rawMaterialIds)
            : new List<RawMaterial>();
        var rawMaterialsById = rawMaterials.ToDictionary(x => x.Id);

        return MapToDetailResponse(batch, product, rawMaterialsById);
    }

    private async Task<Dictionary<ObjectId, Product>> LoadProductsByIdAsync(IEnumerable<ObjectId> ids)
    {
        var objectIds = ids.Distinct().ToList();
        if (objectIds.Count == 0)
            return new Dictionary<ObjectId, Product>();

        var products = await _productRepository.GetByIdsAsync(objectIds);
        return products.ToDictionary(x => x.Id);
    }

    private static List<ProductionBatchIngredient> BuildIngredientSnapshots(
        Product product,
        decimal producedQuantity,
        Dictionary<ObjectId, RawMaterial> rawMaterialsById)
    {
        return product.Recipe.Select(item =>
        {
            var rawMaterial = rawMaterialsById[item.RawMaterialId];
            var quantityUsed = item.Quantity * producedQuantity;
            var cost = quantityUsed * rawMaterial.LastPricePerUnit;

            return new ProductionBatchIngredient
            {
                RawMaterialId = item.RawMaterialId,
                QuantityUsed = quantityUsed,
                PricePerUnit = rawMaterial.LastPricePerUnit,
                Cost = cost
            };
        }).ToList();
    }

    private static List<InsufficientStockItemResponse> BuildInsufficientStockItems(
        List<ProductionBatchIngredient> ingredients,
        Dictionary<ObjectId, RawMaterial> rawMaterialsById,
        Dictionary<ObjectId, Stock> stocksByItemId)
    {
        return ingredients
            .Select(ingredient =>
            {
                stocksByItemId.TryGetValue(ingredient.RawMaterialId, out var stock);
                var available = stock?.CurrentQuantity ?? 0m;
                var missing = ingredient.QuantityUsed - available;
                var rawMaterial = rawMaterialsById[ingredient.RawMaterialId];

                return new InsufficientStockItemResponse
                {
                    RawMaterialId = ingredient.RawMaterialId.ToString(),
                    RawMaterialName = rawMaterial.Name,
                    RequiredQuantity = ingredient.QuantityUsed,
                    AvailableQuantity = available,
                    MissingQuantity = missing
                };
            })
            .Where(x => x.MissingQuantity > 0)
            .ToList();
    }

    private static ProductionBatchListResponse MapToListResponse(ProductionBatch batch, Product? product) => new()
    {
        Id = batch.Id.ToString(),
        ProductId = batch.ProductId.ToString(),
        ProductName = product?.Name ?? string.Empty,
        Quantity = batch.Quantity,
        Date = batch.Date,
        TotalCost = batch.TotalCost,
        UnitCost = batch.UnitCost,
        Notes = batch.Notes,
        CreatedAt = batch.CreatedAt
    };

    private static ProductionBatchDetailResponse MapToDetailResponse(
        ProductionBatch batch,
        Product? product,
        Dictionary<ObjectId, RawMaterial> rawMaterialsById)
    {
        var response = new ProductionBatchDetailResponse
        {
            Id = batch.Id.ToString(),
            ProductId = batch.ProductId.ToString(),
            ProductName = product?.Name ?? string.Empty,
            Quantity = batch.Quantity,
            Date = batch.Date,
            TotalCost = batch.TotalCost,
            UnitCost = batch.UnitCost,
            Notes = batch.Notes,
            CreatedAt = batch.CreatedAt,
            Ingredients = batch.Ingredients.Select(ingredient =>
            {
                rawMaterialsById.TryGetValue(ingredient.RawMaterialId, out var rawMaterial);
                return new ProductionBatchIngredientResponse
                {
                    RawMaterialId = ingredient.RawMaterialId.ToString(),
                    RawMaterialName = rawMaterial?.Name ?? string.Empty,
                    QuantityUsed = ingredient.QuantityUsed,
                    PricePerUnit = ingredient.PricePerUnit,
                    Cost = ingredient.Cost
                };
            }).ToList()
        };

        return response;
    }

    private static void ValidateCreateRequest(CreateProductionBatchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProductId))
            throw new BusinessException("El producto es obligatorio.");

        if (request.Quantity <= 0)
            throw new BusinessException("La cantidad debe ser mayor a 0.");

        if (!request.Date.HasValue || request.Date.Value == default)
            throw new BusinessException("La fecha es obligatoria.");
    }
}
