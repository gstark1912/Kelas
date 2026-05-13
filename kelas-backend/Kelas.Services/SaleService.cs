using Kelas.Domain.Entities;
using Kelas.Domain.Exceptions;
using Kelas.Domain.Interfaces.Repositories;
using Kelas.Domain.Interfaces.Services;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kelas.Services;

public class SaleService : ISaleService
{
    private readonly IMongoClient _client;
    private readonly ISaleRepository _saleRepository;
    private readonly IProductRepository _productRepository;
    private readonly IRawMaterialRepository _rawMaterialRepository;
    private readonly IStockAdjustmentService _stockAdjustmentService;
    private readonly ICashAccountRepository _cashAccountRepository;
    private readonly ICashMovementRepository _cashMovementRepository;

    public SaleService(
        IMongoClient client,
        ISaleRepository saleRepository,
        IProductRepository productRepository,
        IRawMaterialRepository rawMaterialRepository,
        IStockAdjustmentService stockAdjustmentService,
        ICashAccountRepository cashAccountRepository,
        ICashMovementRepository cashMovementRepository)
    {
        _client = client;
        _saleRepository = saleRepository;
        _productRepository = productRepository;
        _rawMaterialRepository = rawMaterialRepository;
        _stockAdjustmentService = stockAdjustmentService;
        _cashAccountRepository = cashAccountRepository;
        _cashMovementRepository = cashMovementRepository;
    }

    public async Task<SaleResponse> CreateAsync(CreateSaleRequest request)
    {
        // 1. Validation
        if (request.Items == null || !request.Items.Any())
            throw new BusinessException("La venta debe tener al menos un ítem.");

        if (string.IsNullOrWhiteSpace(request.Channel))
            throw new BusinessException("El canal es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.PaymentMethod))
            throw new BusinessException("El medio de pago es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.CashAccountId))
            throw new BusinessException("La cuenta de caja es obligatoria.");

        // 2. Fetch dependencies
        var productIds = request.Items.Select(i => ObjectId.Parse(i.ProductId)).ToList();
        var products = await _productRepository.GetByIdsAsync(productIds);
        var productsDict = products.ToDictionary(p => p.Id);

        if (productsDict.Count != productIds.Count)
            throw new BusinessException("Uno o más productos no fueron encontrados.");

        var stocks = await _stockAdjustmentService.GetStockByItemsAsync("FinishedProduct", productIds);
        var stockDict = stocks.ToDictionary(s => s.ItemId, s => s.CurrentQuantity);

        var cashAccount = await _cashAccountRepository.GetByIdAsync(request.CashAccountId);
        if (cashAccount == null || !cashAccount.IsActive)
            throw new NotFoundException("CashAccount", request.CashAccountId);

        // 3. Fetch Raw Materials for COGS
        var rawMaterialIds = products.SelectMany(p => p.Recipe.Select(r => r.RawMaterialId)).Distinct().ToList();
        var rawMaterials = await _rawMaterialRepository.GetByIdsAsync(rawMaterialIds);
        var rawMaterialsDict = rawMaterials.ToDictionary(rm => rm.Id);

        // 4. Calculations
        var saleItems = new List<SaleItem>();
        decimal subtotalProductos = 0;
        decimal totalCOGS = 0;

        foreach (var itemRequest in request.Items)
        {
            var productIdObj = ObjectId.Parse(itemRequest.ProductId);
            var product = productsDict[productIdObj];
            var currentStock = stockDict.GetValueOrDefault(productIdObj, 0m);
            
            if (itemRequest.Quantity > currentStock)
                throw new BusinessException($"Stock insuficiente para el producto '{product.Name}'. Disponible: {currentStock}, Solicitado: {itemRequest.Quantity}.");

            // Calculate Unit Cost (COGS snapshot)
            decimal unitCost = 0;
            foreach (var recipeItem in product.Recipe)
            {
                if (rawMaterialsDict.TryGetValue(recipeItem.RawMaterialId, out var rm))
                {
                    unitCost += recipeItem.Quantity * rm.LastPricePerUnit;
                }
            }

            var itemSubtotal = itemRequest.Quantity * product.ListPrice;
            subtotalProductos += itemSubtotal;
            totalCOGS += itemRequest.Quantity * unitCost;

            saleItems.Add(new SaleItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = itemRequest.Quantity,
                UnitPrice = product.ListPrice,
                UnitCost = unitCost,
                Subtotal = itemSubtotal
            });
        }

        decimal discountAmount = subtotalProductos * (request.DiscountPercent / 100m);
        decimal taxCostAmount = subtotalProductos * (request.TaxCostPercent / 100m);
        decimal channelCostAmount = subtotalProductos * (request.ChannelCostPercent / 100m);
        decimal grossIncome = (subtotalProductos - discountAmount) + request.ShippingCost;
        decimal grossProfit = grossIncome - totalCOGS - request.ShippingCost;
        decimal netProfit = grossProfit - taxCostAmount - channelCostAmount;

        // 5. Transactional Execution
        using var session = await _client.StartSessionAsync();
        session.StartTransaction();
        try
        {
            // a. Create Sale
            var sale = new Sale
            {
                Date = request.Date,
                Channel = request.Channel,
                PaymentMethod = request.PaymentMethod,
                CashAccountId = ObjectId.Parse(request.CashAccountId),
                Items = saleItems,
                SubtotalProductos = subtotalProductos,
                ShippingCost = request.ShippingCost,
                ShippingDetail = request.ShippingDetail,
                DiscountPercent = request.DiscountPercent,
                DiscountAmount = discountAmount,
                TaxCostPercent = request.TaxCostPercent,
                TaxCostAmount = taxCostAmount,
                ChannelCostPercent = request.ChannelCostPercent,
                ChannelCostAmount = channelCostAmount,
                GrossIncome = grossIncome,
                TotalCOGS = totalCOGS,
                GrossProfit = grossProfit,
                NetProfit = netProfit,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow
            };
            var createdSale = await _saleRepository.CreateAsync(sale, session);

            // b. Impact Stock
            foreach (var item in saleItems)
            {
                // Register movement (this also updates stock balance)
                await _stockAdjustmentService.RegisterMovementAsync(
                    itemType: "FinishedProduct",
                    itemId: item.ProductId,
                    movementType: "SaleOutput",
                    quantity: -item.Quantity,
                    date: request.Date,
                    referenceType: "Sale",
                    referenceId: createdSale.Id,
                    session: session
                );
            }

            // c. Impact Cash
            var cashMovement = new CashMovement
            {
                CashAccountId = ObjectId.Parse(request.CashAccountId),
                Type = "income",
                Concept = "Venta",
                Amount = grossIncome,
                Description = $"Venta {createdSale.Id} - Canal: {request.Channel}",
                Date = request.Date,
                Origin = "automatic",
                ReferenceType = "Sale",
                ReferenceId = createdSale.Id,
                CreatedAt = DateTime.UtcNow
            };
            await _cashMovementRepository.CreateAsync(cashMovement, session);

            await _cashAccountRepository.IncrementBalanceAsync(request.CashAccountId, grossIncome, session);

            await session.CommitTransactionAsync();
            return MapToResponse(createdSale);
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    public async Task<List<SaleResponse>> GetByFiltersAsync(DateTime? dateFrom, DateTime? dateTo, string? channel, string? paymentMethod)
    {
        var sales = await _saleRepository.GetByFiltersAsync(dateFrom, dateTo, channel, paymentMethod);
        return sales.Select(MapToResponse).ToList();
    }

    public async Task<SaleResponse> GetByIdAsync(string id)
    {
        var sale = await _saleRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Sale", id);
        return MapToResponse(sale);
    }

    private static SaleResponse MapToResponse(Sale sale) => new()
    {
        Id = sale.Id.ToString(),
        Date = sale.Date,
        Channel = sale.Channel,
        PaymentMethod = sale.PaymentMethod,
        CashAccountId = sale.CashAccountId.ToString(),
        Items = sale.Items.Select(i => new SaleItemResponse
        {
            ProductId = i.ProductId.ToString(),
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            UnitCost = i.UnitCost,
            Subtotal = i.Subtotal
        }).ToList(),
        SubtotalProductos = sale.SubtotalProductos,
        ShippingCost = sale.ShippingCost,
        ShippingDetail = sale.ShippingDetail,
        DiscountPercent = sale.DiscountPercent,
        DiscountAmount = sale.DiscountAmount,
        TaxCostPercent = sale.TaxCostPercent,
        TaxCostAmount = sale.TaxCostAmount,
        ChannelCostPercent = sale.ChannelCostPercent,
        ChannelCostAmount = sale.ChannelCostAmount,
        GrossIncome = sale.GrossIncome,
        TotalCOGS = sale.TotalCOGS,
        GrossProfit = sale.GrossProfit,
        NetProfit = sale.NetProfit,
        Notes = sale.Notes,
        CreatedAt = sale.CreatedAt
    };
}
