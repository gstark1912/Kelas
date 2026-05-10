namespace Kelas.Domain.Models.Requests;

public class CreateRawMaterialRequest
{
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal MinStock { get; set; }
}
