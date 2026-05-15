namespace Kelas.Domain.Models.Responses;

public class CashTransferResponse
{
    public CashMovementResponse OriginMovement { get; set; } = new();
    public CashMovementResponse DestinationMovement { get; set; } = new();
}
