namespace Kelas.Domain.Models.Responses;

public class ProductionBatchListResultResponse
{
    public List<ProductionBatchListResponse> Items { get; set; } = new();
    public ProductionKpisResponse Kpis { get; set; } = new();
}

public class ProductionBatchListResponse
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public DateTime Date { get; set; }
    public decimal TotalCost { get; set; }
    public decimal UnitCost { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProductionBatchDetailResponse : ProductionBatchListResponse
{
    public List<ProductionBatchIngredientResponse> Ingredients { get; set; } = new();
}

public class ProductionBatchIngredientResponse
{
    public string RawMaterialId { get; set; } = string.Empty;
    public string RawMaterialName { get; set; } = string.Empty;
    public decimal QuantityUsed { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal Cost { get; set; }
}

public class ProductionKpisResponse
{
    public decimal TotalUnitsProduced { get; set; }
    public decimal TotalCost { get; set; }
}

public class InsufficientStockItemResponse
{
    public string RawMaterialId { get; set; } = string.Empty;
    public string RawMaterialName { get; set; } = string.Empty;
    public decimal RequiredQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal MissingQuantity { get; set; }
}
