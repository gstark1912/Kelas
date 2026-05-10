using Kelas.Domain.Entities;
using Kelas.Domain.Exceptions;
using Kelas.Domain.Interfaces.Repositories;
using Kelas.Domain.Interfaces.Services;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kelas.Services;

public class RawMaterialService : IRawMaterialService
{
    private readonly IRawMaterialRepository _rawMaterialRepository;
    private readonly IStockRepository _stockRepository;
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _database;

    private static readonly HashSet<string> ValidUnits = new()
    {
        "gr", "kg", "ml", "lt", "unidad", "cm"
    };

    public RawMaterialService(
        IRawMaterialRepository rawMaterialRepository,
        IStockRepository stockRepository,
        IMongoClient client,
        IMongoDatabase database)
    {
        _rawMaterialRepository = rawMaterialRepository;
        _stockRepository = stockRepository;
        _client = client;
        _database = database;
    }

    public async Task<List<RawMaterialListResponse>> GetAllAsync(string? search = null, string? unit = null)
    {
        var materials = await _rawMaterialRepository.GetActiveAsync(search, unit);
        var stocks = await _stockRepository.GetByItemTypeAsync("RawMaterial");
        var stockDict = stocks.ToDictionary(s => s.ItemId.ToString(), s => s);

        return materials.Select(m =>
        {
            stockDict.TryGetValue(m.Id.ToString(), out var stock);
            var currentQuantity = stock?.CurrentQuantity ?? 0;

            return new RawMaterialListResponse
            {
                Id = m.Id.ToString(),
                Name = m.Name,
                Unit = m.Unit,
                MinStock = m.MinStock,
                CurrentQuantity = currentQuantity,
                LastPricePerUnit = m.LastPricePerUnit,
                Status = CalculateStatus(currentQuantity, m.MinStock),
                LastPurchaseDate = null
            };
        }).ToList();
    }

    public async Task<RawMaterialDetailResponse> GetByIdAsync(string id)
    {
        var entity = await _rawMaterialRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("RawMaterial", id);

        var stock = await _stockRepository.GetByItemAsync("RawMaterial", id);
        var currentQuantity = stock?.CurrentQuantity ?? 0;

        return MapToDetailResponse(entity, currentQuantity);
    }

    public async Task<RawMaterialDetailResponse> CreateAsync(CreateRawMaterialRequest request)
    {
        ValidateName(request.Name);
        ValidateUnit(request.Unit);
        ValidateMinStock(request.MinStock);

        var trimmedName = request.Name.Trim();

        var existing = await _rawMaterialRepository.GetByNameAsync(trimmedName);
        if (existing is not null)
            throw new BusinessException($"Ya existe una materia prima con el nombre '{existing.Name}'.");

        var entity = new RawMaterial
        {
            Name = trimmedName,
            Unit = request.Unit,
            MinStock = request.MinStock,
            LastPricePerUnit = 0,
            IsActive = true
        };

        using var session = await _client.StartSessionAsync();
        session.StartTransaction();
        try
        {
            var created = await _rawMaterialRepository.CreateAsync(entity, session);

            var stock = new Stock
            {
                ItemType = "RawMaterial",
                ItemId = created.Id,
                CurrentQuantity = 0
            };
            await _stockRepository.CreateAsync(stock, session);

            await session.CommitTransactionAsync();

            return MapToDetailResponse(created, 0);
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    public async Task<RawMaterialDetailResponse> UpdateAsync(string id, UpdateRawMaterialRequest request)
    {
        var entity = await _rawMaterialRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("RawMaterial", id);

        ValidateName(request.Name);
        ValidateUnit(request.Unit);
        ValidateMinStock(request.MinStock);

        var trimmedName = request.Name.Trim();

        var existing = await _rawMaterialRepository.GetByNameAsync(trimmedName);
        if (existing is not null && existing.Id != entity.Id)
            throw new BusinessException($"Ya existe una materia prima con el nombre '{existing.Name}'.");

        entity.Name = trimmedName;
        entity.Unit = request.Unit;
        entity.MinStock = request.MinStock;

        await _rawMaterialRepository.UpdateAsync(id, entity);

        var stock = await _stockRepository.GetByItemAsync("RawMaterial", id);
        var currentQuantity = stock?.CurrentQuantity ?? 0;

        return MapToDetailResponse(entity, currentQuantity);
    }

    public async Task DeleteAsync(string id)
    {
        var entity = await _rawMaterialRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("RawMaterial", id);

        var stock = await _stockRepository.GetByItemAsync("RawMaterial", id);
        if (stock is not null && stock.CurrentQuantity > 0)
            throw new BusinessException("No se puede eliminar una materia prima con stock.");

        var productsCollection = _database.GetCollection<BsonDocument>("products");
        var objectId = ObjectId.Parse(id);
        var filter = Builders<BsonDocument>.Filter.And(
            Builders<BsonDocument>.Filter.Eq("isActive", true),
            Builders<BsonDocument>.Filter.ElemMatch<BsonDocument>("recipe",
                Builders<BsonDocument>.Filter.Eq("rawMaterialId", objectId))
        );
        var referencedInRecipe = await productsCollection.Find(filter).AnyAsync();
        if (referencedInRecipe)
            throw new BusinessException("No se puede eliminar una materia prima que está en uso en recetas.");

        entity.IsActive = false;
        await _rawMaterialRepository.UpdateAsync(id, entity);
    }

    private static string CalculateStatus(decimal currentQuantity, decimal minStock)
    {
        if (currentQuantity == 0) return "Sin stock";
        if (currentQuantity < minStock) return "Bajo";
        return "OK";
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("El nombre es obligatorio.");
    }

    private static void ValidateUnit(string unit)
    {
        if (!ValidUnits.Contains(unit))
            throw new BusinessException($"La unidad '{unit}' no es válida. Unidades permitidas: gr, kg, ml, lt, unidad, cm.");
    }

    private static void ValidateMinStock(decimal minStock)
    {
        if (minStock < 0)
            throw new BusinessException("El stock mínimo debe ser mayor o igual a 0.");
    }

    private static RawMaterialDetailResponse MapToDetailResponse(RawMaterial entity, decimal currentQuantity) => new()
    {
        Id = entity.Id.ToString(),
        Name = entity.Name,
        Unit = entity.Unit,
        MinStock = entity.MinStock,
        CurrentQuantity = currentQuantity,
        LastPricePerUnit = entity.LastPricePerUnit,
        Status = CalculateStatus(currentQuantity, entity.MinStock),
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
