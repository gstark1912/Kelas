using Kelas.Domain.Entities;
using Kelas.Domain.Exceptions;
using Kelas.Domain.Interfaces.Repositories;
using Kelas.Domain.Interfaces.Services;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kelas.Services;

public class PurchaseService : IPurchaseService
{
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _database;
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly IRawMaterialRepository _rawMaterialRepository;
    private readonly IStockRepository _stockRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly ICashMovementRepository _cashMovementRepository;
    private readonly IRawMaterialPriceRepository _rawMaterialPriceRepository;
    private readonly ICashAccountRepository _cashAccountRepository;

    public PurchaseService(
        IMongoClient client,
        IMongoDatabase database,
        IPurchaseRepository purchaseRepository,
        IRawMaterialRepository rawMaterialRepository,
        IStockRepository stockRepository,
        IStockMovementRepository stockMovementRepository,
        ICashMovementRepository cashMovementRepository,
        IRawMaterialPriceRepository rawMaterialPriceRepository,
        ICashAccountRepository cashAccountRepository)
    {
        _client = client;
        _database = database;
        _purchaseRepository = purchaseRepository;
        _rawMaterialRepository = rawMaterialRepository;
        _stockRepository = stockRepository;
        _stockMovementRepository = stockMovementRepository;
        _cashMovementRepository = cashMovementRepository;
        _rawMaterialPriceRepository = rawMaterialPriceRepository;
        _cashAccountRepository = cashAccountRepository;
    }

    public async Task<PurchaseResponse> CreateAsync(CreatePurchaseRequest request)
    {
        // Validate inputs
        if (request.Quantity <= 0)
            throw new BusinessException("La cantidad debe ser mayor a 0.");

        if (request.TotalPrice <= 0)
            throw new BusinessException("El precio total debe ser mayor a 0.");

        if (!request.Date.HasValue || request.Date.Value == default)
            throw new BusinessException("La fecha es obligatoria.");

        // Look up raw material
        var rawMaterial = await _rawMaterialRepository.GetByIdAsync(request.RawMaterialId);
        if (rawMaterial is null || !rawMaterial.IsActive)
            throw new NotFoundException("RawMaterial", request.RawMaterialId);

        // Look up cash account
        var cashAccount = await _cashAccountRepository.GetByIdAsync(request.CashAccountId);
        if (cashAccount is null || !cashAccount.IsActive)
            throw new NotFoundException("CashAccount", request.CashAccountId);

        // Calculate price per unit
        var pricePerUnit = request.TotalPrice / request.Quantity;

        // Start transaction
        using var session = await _client.StartSessionAsync();
        session.StartTransaction();
        try
        {
            // a. Create Purchase entity
            var purchase = new Purchase
            {
                RawMaterialId = rawMaterial.Id,
                Quantity = request.Quantity,
                TotalPrice = request.TotalPrice,
                PricePerUnit = pricePerUnit,
                Date = request.Date.Value,
                Supplier = request.Supplier,
                CashAccountId = ObjectId.Parse(request.CashAccountId),
                Notes = request.Notes,
                SkipPriceUpdate = request.SkipPriceUpdate,
                CreatedAt = DateTime.UtcNow
            };
            var createdPurchase = await _purchaseRepository.CreateAsync(purchase, session);

            // b. Update raw material's LastPricePerUnit (skip if flagged as special purchase)
            if (!request.SkipPriceUpdate)
            {
                var rawMaterialsCollection = _database.GetCollection<RawMaterial>("rawMaterials");
                var rmFilter = Builders<RawMaterial>.Filter.Eq(x => x.Id, rawMaterial.Id);
                var rmUpdate = Builders<RawMaterial>.Update
                    .Set(x => x.LastPricePerUnit, pricePerUnit)
                    .Set(x => x.UpdatedAt, DateTime.UtcNow);
                await rawMaterialsCollection.UpdateOneAsync(session, rmFilter, rmUpdate);

                // c. Create RawMaterialPrice record
                var rawMaterialPrice = new RawMaterialPrice
                {
                    RawMaterialId = rawMaterial.Id,
                    PricePerUnit = pricePerUnit,
                    DateFrom = request.Date.Value,
                    PurchaseId = createdPurchase.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _rawMaterialPriceRepository.CreateAsync(rawMaterialPrice, session);
            }

            // d. Create StockMovement
            var stockMovement = new StockMovement
            {
                ItemType = "RawMaterial",
                ItemId = rawMaterial.Id,
                MovementType = "PurchaseEntry",
                Quantity = request.Quantity,
                Date = request.Date.Value,
                ReferenceType = "Purchase",
                ReferenceId = createdPurchase.Id,
                CreatedAt = DateTime.UtcNow
            };
            await _stockMovementRepository.CreateAsync(stockMovement, session);

            // e. Increment stock quantity
            await _stockRepository.IncrementQuantityAsync("RawMaterial", rawMaterial.Id.ToString(), request.Quantity, session);

            // f. Create CashMovement
            var cashMovement = new CashMovement
            {
                CashAccountId = ObjectId.Parse(request.CashAccountId),
                Type = "expense",
                Concept = "Compra MP",
                Amount = request.TotalPrice,
                Description = $"Compra de {rawMaterial.Name}",
                Date = request.Date.Value,
                Origin = "automatic",
                ReferenceType = "Purchase",
                ReferenceId = createdPurchase.Id,
                CreatedAt = DateTime.UtcNow
            };
            await _cashMovementRepository.CreateAsync(cashMovement, session);

            // g. Decrement cash balance
            await _cashAccountRepository.DecrementBalanceAsync(request.CashAccountId, request.TotalPrice, session);

            // Commit transaction
            await session.CommitTransactionAsync();

            // Return response
            return MapToResponse(createdPurchase, rawMaterial.Name, cashAccount.Name);
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    public async Task<List<PurchaseResponse>> GetByRawMaterialAsync(string rawMaterialId)
    {
        var purchases = await _purchaseRepository.GetByRawMaterialIdAsync(rawMaterialId);

        if (purchases.Count == 0)
            return new List<PurchaseResponse>();

        // Load raw material once
        var rawMaterial = await _rawMaterialRepository.GetByIdAsync(rawMaterialId);
        var rawMaterialName = rawMaterial?.Name ?? string.Empty;

        // Load all unique cash accounts
        var cashAccountIds = purchases.Select(p => p.CashAccountId.ToString()).Distinct().ToList();
        var cashAccountNames = new Dictionary<string, string>();
        foreach (var cashAccountId in cashAccountIds)
        {
            var account = await _cashAccountRepository.GetByIdAsync(cashAccountId);
            cashAccountNames[cashAccountId] = account?.Name ?? string.Empty;
        }

        return purchases.Select(p =>
        {
            cashAccountNames.TryGetValue(p.CashAccountId.ToString(), out var cashAccountName);
            return MapToResponse(p, rawMaterialName, cashAccountName ?? string.Empty);
        }).ToList();
    }

    private static PurchaseResponse MapToResponse(Purchase purchase, string rawMaterialName, string cashAccountName) => new()
    {
        Id = purchase.Id.ToString(),
        RawMaterialId = purchase.RawMaterialId.ToString(),
        RawMaterialName = rawMaterialName,
        Quantity = purchase.Quantity,
        TotalPrice = purchase.TotalPrice,
        PricePerUnit = purchase.PricePerUnit,
        Date = purchase.Date,
        Supplier = purchase.Supplier,
        CashAccountId = purchase.CashAccountId.ToString(),
        CashAccountName = cashAccountName,
        Notes = purchase.Notes,
        CreatedAt = purchase.CreatedAt
    };
}
