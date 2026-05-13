using Kelas.Domain.Entities;
using Kelas.Domain.Exceptions;
using Kelas.Domain.Interfaces.Repositories;
using Kelas.Domain.Interfaces.Services;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kelas.Services;

public class StockAdjustmentService : IStockAdjustmentService
{
    private static readonly string[] ValidReasons =
    {
        "Vencimiento",
        "Rotura",
        "Pérdida",
        "Corrección de inventario",
        "Otro"
    };

    private readonly IMongoClient _client;
    private readonly IRawMaterialRepository _rawMaterialRepository;
    private readonly IProductRepository _productRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly IStockRepository _stockRepository;

    public StockAdjustmentService(
        IMongoClient client,
        IRawMaterialRepository rawMaterialRepository,
        IProductRepository productRepository,
        IStockMovementRepository stockMovementRepository,
        IStockRepository stockRepository)
    {
        _client = client;
        _rawMaterialRepository = rawMaterialRepository;
        _productRepository = productRepository;
        _stockMovementRepository = stockMovementRepository;
        _stockRepository = stockRepository;
    }

    public async Task<StockAdjustmentResponse> CreateAsync(CreateStockAdjustmentRequest request)
    {
        // Validate reason
        if (string.IsNullOrWhiteSpace(request.Reason) || !ValidReasons.Contains(request.Reason))
            throw new BusinessException("El motivo es obligatorio y debe ser uno de los valores permitidos: Vencimiento, Rotura, Pérdida, Corrección de inventario, Otro.");

        // Validate date
        if (!request.Date.HasValue || request.Date.Value == default)
            throw new BusinessException("La fecha es obligatoria.");

        ValidateItemType(request.ItemType);
        ValidateItemId(request.ItemId);

        // Validate newStock is not negative
        if (request.NewStock < 0)
            throw new BusinessException("El nuevo stock no puede ser negativo.");

        var item = await GetAdjustableItemAsync(request.ItemType, request.ItemId);

        // Read current stock and calculate delta
        var currentStock = await _stockRepository.GetByItemAsync(request.ItemType, item.Id.ToString());
        var currentQuantity = currentStock?.CurrentQuantity ?? 0m;
        var delta = request.NewStock - currentQuantity;

        // Delta must not be zero
        if (delta == 0)
            throw new BusinessException("El nuevo stock es igual al stock actual. No hay cambio que registrar.");

        // Determine movement type based on delta sign
        var movementType = delta > 0 ? "AdjustmentIncrease" : "AdjustmentDecrease";

        // Start transaction
        using var session = await _client.StartSessionAsync();
        session.StartTransaction();
        try
        {
            var createdMovement = await RegisterMovementAsync(
                request.ItemType,
                item.Id,
                movementType,
                delta,
                request.Date.Value,
                adjustmentReason: request.Reason,
                notes: request.Notes,
                session: session);

            // Commit transaction
            await session.CommitTransactionAsync();

            // Warning if resulting stock is negative (shouldn't happen with validation above, but kept for safety)
            string? warning = request.NewStock < 0 ? "El stock quedó en valor negativo" : null;

            return new StockAdjustmentResponse
            {
                Id = createdMovement.Id.ToString(),
                ItemType = request.ItemType,
                ItemId = item.Id.ToString(),
                ItemName = item.Name,
                MovementType = movementType,
                Quantity = delta,
                AdjustmentReason = request.Reason,
                Date = request.Date.Value,
                Notes = request.Notes,
                NewStock = request.NewStock,
                Warning = warning,
                CreatedAt = createdMovement.CreatedAt
            };
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    public Task<Stock?> GetStockByItemAsync(string itemType, string itemId)
    {
        ValidateItemType(itemType);
        ValidateItemId(itemId);

        return _stockRepository.GetByItemAsync(itemType, itemId);
    }

    public Task<List<Stock>> GetStockByItemsAsync(string itemType, IEnumerable<ObjectId> itemIds)
    {
        ValidateItemType(itemType);

        return _stockRepository.GetByItemsAsync(itemType, itemIds);
    }

    public async Task<StockMovement> RegisterMovementAsync(
        string itemType,
        ObjectId itemId,
        string movementType,
        decimal quantity,
        DateTime date,
        string? referenceType = null,
        ObjectId? referenceId = null,
        string? adjustmentReason = null,
        string? notes = null,
        object? session = null)
    {
        ValidateItemType(itemType);

        if (string.IsNullOrWhiteSpace(movementType))
            throw new BusinessException("El tipo de movimiento es obligatorio.");

        if (quantity == 0)
            throw new BusinessException("La cantidad del movimiento no puede ser cero.");

        if (date == default)
            throw new BusinessException("La fecha es obligatoria.");

        var stockMovement = new StockMovement
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
        };

        var createdMovement = await _stockMovementRepository.CreateAsync(stockMovement, session);
        await _stockRepository.UpsertIncrementQuantityAsync(itemType, itemId, quantity, session);

        return createdMovement;
    }

    public async Task<List<StockMovementResponse>> GetMovementsByItemAsync(string itemType, string itemId)
    {
        ValidateItemType(itemType);
        ValidateItemId(itemId);

        var movements = await _stockMovementRepository.GetByItemAsync(itemType, itemId);

        return movements.Select(m => new StockMovementResponse
        {
            Id = m.Id.ToString(),
            MovementType = m.MovementType,
            Quantity = m.Quantity,
            Date = m.Date,
            ReferenceType = m.ReferenceType,
            ReferenceId = m.ReferenceId?.ToString(),
            AdjustmentReason = m.AdjustmentReason,
            Notes = m.Notes,
            CreatedAt = m.CreatedAt
        }).ToList();
    }

    private static void ValidateItemType(string itemType)
    {
        if (string.IsNullOrWhiteSpace(itemType))
            throw new BusinessException("El tipo de ítem es obligatorio.");

        if (itemType is not "RawMaterial" and not "FinishedProduct")
            throw new BusinessException("El tipo de ítem debe ser RawMaterial o FinishedProduct.");
    }

    private static void ValidateItemId(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            throw new BusinessException("El identificador de ítem es obligatorio.");

        if (!ObjectId.TryParse(itemId, out _))
            throw new BusinessException("El identificador de ítem no es válido.");
    }

    private async Task<AdjustableItem> GetAdjustableItemAsync(string itemType, string itemId)
    {
        if (itemType == "RawMaterial")
        {
            var rawMaterial = await _rawMaterialRepository.GetByIdAsync(itemId);
            if (rawMaterial is null || !rawMaterial.IsActive)
                throw new NotFoundException("RawMaterial", itemId);

            return new AdjustableItem(rawMaterial.Id, rawMaterial.Name);
        }

        var product = await _productRepository.GetByIdAsync(itemId);
        if (product is null || !product.IsVisible)
            throw new NotFoundException("Product", itemId);

        return new AdjustableItem(product.Id, product.Name);
    }

    private sealed record AdjustableItem(ObjectId Id, string Name);
}
