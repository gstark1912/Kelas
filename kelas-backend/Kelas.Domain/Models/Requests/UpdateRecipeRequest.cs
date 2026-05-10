namespace Kelas.Domain.Models.Requests;

public class UpdateRecipeRequest
{
    public List<RecipeItemRequest> Ingredients { get; set; } = new();
}

public class RecipeItemRequest
{
    public string RawMaterialId { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}
