namespace Kelas.Domain.Models.Responses;

public class HealthCheckResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> Services { get; set; } = new();
    public string? Error { get; set; }
}
