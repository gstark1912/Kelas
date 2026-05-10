using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Kelas.Domain.Configuration;
using Kelas.Domain.Exceptions;
using Kelas.Domain.Interfaces.Services;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Kelas.Services;

public class AuthService : IAuthService
{
    private readonly AuthSettings _authSettings;
    private readonly JwtSettings _jwtSettings;

    public AuthService(IOptions<AuthSettings> authSettings, IOptions<JwtSettings> jwtSettings)
    {
        _authSettings = authSettings.Value;
        _jwtSettings = jwtSettings.Value;
    }

    public Task<LoginResponse> AuthenticateAsync(LoginRequest request)
    {
        if (!string.Equals(request.Email, _authSettings.Email, StringComparison.OrdinalIgnoreCase)
            || request.Password != _authSettings.Password)
        {
            throw new UnauthorizedException("Credenciales inválidas.");
        }

        var token = GenerateToken(request.Email);
        return Task.FromResult(new LoginResponse { Token = token });
    }

    private string GenerateToken(string email)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
