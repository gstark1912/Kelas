using Kelas.Domain.Entities;
using Kelas.Domain.Interfaces.Repositories;
using Kelas.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Kelas.Services;

public class CashAccountSeeder : ICashAccountSeeder
{
    private readonly ICashAccountRepository _repository;
    private readonly ILogger<CashAccountSeeder> _logger;

    private static readonly string[] DefaultAccountNames = ["Efectivo", "Banco", "Mercado Pago"];

    public CashAccountSeeder(ICashAccountRepository repository, ILogger<CashAccountSeeder> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            await _repository.EnsureIndexesAsync();

            foreach (var name in DefaultAccountNames)
            {
                var existing = await _repository.GetByNameAsync(name);
                if (existing is not null)
                {
                    _logger.LogDebug("Cuenta de caja '{Name}' ya existe, omitiendo.", name);
                    continue;
                }

                var entity = new CashAccount
                {
                    Name = name,
                    CurrentBalance = 0,
                    IsActive = true
                };

                await _repository.CreateAsync(entity);
                _logger.LogInformation("Cuenta de caja '{Name}' creada exitosamente.", name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el seed de cuentas de caja. La aplicación continuará su inicio.");
        }
    }
}
