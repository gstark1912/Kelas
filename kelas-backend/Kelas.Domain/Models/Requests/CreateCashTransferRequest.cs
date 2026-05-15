namespace Kelas.Domain.Models.Requests;

public class CreateCashTransferRequest
{
    public string OriginCashAccountId { get; set; } = string.Empty;
    public string DestinationCashAccountId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
}
