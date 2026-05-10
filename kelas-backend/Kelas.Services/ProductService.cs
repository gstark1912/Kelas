using Kelas.Domain.Entities;
using Kelas.Domain.Exceptions;
using Kelas.Domain.Interfaces.Repositories;
using Kelas.Domain.Interfaces.Services;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kelas.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IStockRepository _stockRepository;
    private readonly IRawMaterialRepository _rawMaterialRepository;
    private readonly IMongoClient _client;

    public ProductService(
        IProductRepository productRepository,
        IStockRepository stockRepository,
        IRawMaterialRepository rawMaterialRepository,
        IMongoClient client)
    {
        _productRepository = productRepository;
        _stockRepository = stockRepository;
        _rawMaterialRepository = rawMaterialRepository;
        _client = client;
    }

    public async Task<List<ProductListResponse>> GetAllAsync(string? search = null)
    {
        var products = await _productRepository.GetVisibleAsync(search);

        if (!products.Any())
            return new List<ProductListResponse>();

        // Collect all raw material IDs across all recipes — one batch query
        var allRawMaterialIds = products
            .SelectMany(p => p.Recipe.Select(r => r.RawMaterialId))
            .Distinct()
            .ToList();

        var rawMaterials = allRawMaterialIds.Any()
            ? await _rawMaterialRepository.GetByIdsAsync(allRawMaterialIds)
            : new List<RawMaterial>();

        var rawMaterialsDict = rawMaterials.ToDictionary(x => x.Id);

        // Get all stock for finished products
        var stocks = await _stockRepository.GetByItemTypeAsync("FinishedProduct");
        var stockDict = stocks.ToDictionary(s => s.ItemId.ToString(), s => s);

        return products.Select(p =>
        {
            var estimatedCost = CalculateEstimatedCost(p.Recipe, rawMaterialsDict);
            var margin = CalculateMargin(p.ListPrice, estimatedCost);
            var marginAlert = CalculateMarginAlert(margin, p.MinMargin);

            stockDict.TryGetValue(p.Id.ToString(), out var stock);
            var currentStock = stock?.CurrentQuantity ?? 0;

            return new ProductListResponse
            {
                Id = p.Id.ToString(),
                Name = p.Name,
                Description = p.Description,
                ListPrice = p.ListPrice,
                EstimatedHours = p.EstimatedHours,
                MinMargin = p.MinMargin,
                EstimatedCost = estimatedCost,
                Margin = margin,
                CurrentStock = currentStock,
                MarginAlert = marginAlert
            };
        }).ToList();
    }

    public async Task<ProductDetailResponse> GetByIdAsync(string id)
    {
        var product = await _productRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Product", id);

        var stock = await _stockRepository.GetByItemAsync("FinishedProduct", id);
        var currentStock = stock?.CurrentQuantity ?? 0;

        var rawMaterialIds = product.Recipe.Select(r => r.RawMaterialId).ToList();
        var rawMaterials = rawMaterialIds.Any()
            ? await _rawMaterialRepository.GetByIdsAsync(rawMaterialIds)
            : new List<RawMaterial>();

        var rawMaterialsDict = rawMaterials.ToDictionary(x => x.Id);

        var estimatedCost = CalculateEstimatedCost(product.Recipe, rawMaterialsDict);
        var margin = CalculateMargin(product.ListPrice, estimatedCost);
        var marginAlert = CalculateMarginAlert(margin, product.MinMargin);

        var recipeResponse = product.Recipe.Select(item =>
        {
            rawMaterialsDict.TryGetValue(item.RawMaterialId, out var rm);
            var pricePerUnit = rm?.LastPricePerUnit ?? 0m;
            return new RecipeItemResponse
            {
                RawMaterialId = item.RawMaterialId.ToString(),
                RawMaterialName = rm?.Name ?? string.Empty,
                Unit = rm?.Unit ?? string.Empty,
                Quantity = item.Quantity,
                PricePerUnit = pricePerUnit,
                Subtotal = item.Quantity * pricePerUnit
            };
        }).ToList();

        return new ProductDetailResponse
        {
            Id = product.Id.ToString(),
            Name = product.Name,
            Description = product.Description,
            ListPrice = product.ListPrice,
            EstimatedHours = product.EstimatedHours,
            MinMargin = product.MinMargin,
            EstimatedCost = estimatedCost,
            Margin = margin,
            CurrentStock = currentStock,
            MarginAlert = marginAlert,
            Recipe = recipeResponse,
            IsVisible = product.IsVisible,
            Warning = null,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    public async Task<ProductDetailResponse> CreateAsync(CreateProductRequest request)
    {
        ValidateName(request.Name);
        ValidateListPrice(request.ListPrice);

        var trimmedName = request.Name.Trim();

        var existing = await _productRepository.GetByNameAsync(trimmedName);
        if (existing is not null)
            throw new BusinessException($"Ya existe un producto con el nombre '{existing.Name}'.");

        var entity = new Product
        {
            Name = trimmedName,
            Description = request.Description?.Trim(),
            ListPrice = request.ListPrice,
            EstimatedHours = request.EstimatedHours,
            MinMargin = request.MinMargin,
            IsVisible = true,
            Recipe = new List<RecipeItem>()
        };

        using var session = await _client.StartSessionAsync();
        session.StartTransaction();
        try
        {
            var created = await _productRepository.CreateAsync(entity, session);

            var stock = new Stock
            {
                ItemType = "FinishedProduct",
                ItemId = created.Id,
                CurrentQuantity = 0
            };
            await _stockRepository.CreateAsync(stock, session);

            await session.CommitTransactionAsync();

            return new ProductDetailResponse
            {
                Id = created.Id.ToString(),
                Name = created.Name,
                Description = created.Description,
                ListPrice = created.ListPrice,
                EstimatedHours = created.EstimatedHours,
                MinMargin = created.MinMargin,
                EstimatedCost = 0,
                Margin = 0,
                CurrentStock = 0,
                MarginAlert = "ok",
                Recipe = new List<RecipeItemResponse>(),
                IsVisible = created.IsVisible,
                Warning = null,
                CreatedAt = created.CreatedAt,
                UpdatedAt = created.UpdatedAt
            };
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    public async Task<ProductDetailResponse> UpdateAsync(string id, UpdateProductRequest request)
    {
        var entity = await _productRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Product", id);

        ValidateName(request.Name);
        ValidateListPrice(request.ListPrice);

        var trimmedName = request.Name.Trim();

        var existing = await _productRepository.GetByNameAsync(trimmedName);
        if (existing is not null && existing.Id != entity.Id)
            throw new BusinessException($"Ya existe un producto con el nombre '{existing.Name}'.");

        entity.Name = trimmedName;
        entity.Description = request.Description?.Trim();
        entity.ListPrice = request.ListPrice;
        entity.EstimatedHours = request.EstimatedHours;
        entity.MinMargin = request.MinMargin;

        await _productRepository.UpdateAsync(id, entity);

        return await GetByIdAsync(id);
    }

    public async Task<ProductDetailResponse> UpdateRecipeAsync(string id, UpdateRecipeRequest request)
    {
        var entity = await _productRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Product", id);

        if (request.Ingredients == null || !request.Ingredients.Any())
            throw new BusinessException("La receta debe contener al menos un ingrediente.");

        // Validate no duplicate rawMaterialIds
        var rawMaterialIdStrings = request.Ingredients.Select(i => i.RawMaterialId).ToList();
        var duplicates = rawMaterialIdStrings
            .GroupBy(x => x.ToLowerInvariant())
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Any())
            throw new BusinessException("Una materia prima no puede aparecer dos veces en la misma receta.");

        // Validate all quantities > 0
        var invalidQuantity = request.Ingredients.FirstOrDefault(i => i.Quantity <= 0);
        if (invalidQuantity is not null)
            throw new BusinessException("La cantidad de cada ingrediente debe ser mayor a 0.");

        // Parse all ObjectIds
        var rawMaterialObjectIds = new List<ObjectId>();
        foreach (var ingredient in request.Ingredients)
        {
            if (!ObjectId.TryParse(ingredient.RawMaterialId, out var objectId))
                throw new BusinessException($"El identificador de materia prima '{ingredient.RawMaterialId}' no es válido.");
            rawMaterialObjectIds.Add(objectId);
        }

        // Fetch all raw materials in one query
        var rawMaterials = await _rawMaterialRepository.GetByIdsAsync(rawMaterialObjectIds);

        if (rawMaterials.Count != rawMaterialObjectIds.Count)
        {
            var foundIds = rawMaterials.Select(r => r.Id.ToString()).ToHashSet();
            var missingId = rawMaterialObjectIds.First(id => !foundIds.Contains(id.ToString()));
            throw new BusinessException($"La materia prima con id '{missingId}' no fue encontrada.");
        }

        // Build recipe items
        entity.Recipe = request.Ingredients.Select((ingredient, index) => new RecipeItem
        {
            RawMaterialId = rawMaterialObjectIds[index],
            Quantity = ingredient.Quantity
        }).ToList();

        await _productRepository.UpdateAsync(id, entity);

        return await GetByIdAsync(id);
    }

    public async Task<ProductDetailResponse> UpdateVisibilityAsync(string id, UpdateVisibilityRequest request)
    {
        var entity = await _productRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Product", id);

        string? warning = null;

        if (!request.IsVisible)
        {
            var stock = await _stockRepository.GetByItemAsync("FinishedProduct", id);
            if (stock is not null && stock.CurrentQuantity > 0)
            {
                warning = "El producto tiene stock disponible. Se ocultará de los listados operativos pero el stock se conserva.";
            }
        }

        entity.IsVisible = request.IsVisible;
        await _productRepository.UpdateAsync(id, entity);

        var response = await GetByIdAsync(id);
        response.Warning = warning;
        return response;
    }

    // --- Private calculation helpers ---

    private static decimal CalculateEstimatedCost(
        List<RecipeItem> recipe,
        Dictionary<ObjectId, RawMaterial> rawMaterialsDict)
    {
        if (!recipe.Any()) return 0m;

        return recipe.Sum(item =>
            rawMaterialsDict.TryGetValue(item.RawMaterialId, out var rm)
                ? item.Quantity * rm.LastPricePerUnit
                : 0m);
    }

    private static decimal CalculateMargin(decimal listPrice, decimal estimatedCost)
    {
        if (listPrice <= 0) return 0m;
        return (listPrice - estimatedCost) / listPrice * 100m;
    }

    private static string CalculateMarginAlert(decimal margin, decimal? minMargin)
    {
        if (!minMargin.HasValue || minMargin.Value <= 0) return "ok";
        if (margin < minMargin.Value) return "danger";
        if (margin < minMargin.Value * 1.10m) return "warning";
        return "ok";
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("El nombre es obligatorio.");
    }

    private static void ValidateListPrice(decimal listPrice)
    {
        if (listPrice <= 0)
            throw new BusinessException("El precio de lista debe ser mayor a 0.");
    }
}
