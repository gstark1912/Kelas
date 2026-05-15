namespace Kelas.Domain.Models.Responses;

public class CashAccountSummaryResponse
{
    public List<CashAccountResponse> Accounts { get; set; } = new();
    public decimal TotalBalance { get; set; }
}
