namespace Kelas.Domain.Configuration;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public int ExpirationHours { get; set; } = 24;
}
