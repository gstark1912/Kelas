using Kelas.Domain.Configuration;
using Kelas.Domain.Interfaces.Repositories;
using Kelas.Domain.Interfaces.Services;
using Kelas.Repositories;
using Kelas.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Kelas.IoC.Resolver;

public static class DependencyResolver
{
    public static IServiceCollection AddKelasServices(this IServiceCollection services, IConfiguration configuration)
    {
        // MongoDB Configuration
        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDb"));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });

        services.AddScoped<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return client.GetDatabase(settings.DatabaseName);
        });

        // Auth Configuration
        services.Configure<AuthSettings>(configuration.GetSection("Auth"));
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        // Repositories
        AddRepositories(services);

        // Services
        AddServices(services);

        return services;
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<ICashAccountRepository, CashAccountRepository>();
        services.AddScoped<IRawMaterialRepository, RawMaterialRepository>();
        services.AddScoped<IStockRepository, StockRepository>();
        services.AddScoped<IPurchaseRepository, PurchaseRepository>();
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();
        services.AddScoped<ICashMovementRepository, CashMovementRepository>();
        services.AddScoped<IRawMaterialPriceRepository, RawMaterialPriceRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
    }

    private static void AddServices(IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICashAccountService, CashAccountService>();
        services.AddScoped<ICashAccountSeeder, CashAccountSeeder>();
        services.AddScoped<IRawMaterialService, RawMaterialService>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        services.AddScoped<IStockAdjustmentService, StockAdjustmentService>();
        services.AddScoped<IProductService, ProductService>();
    }
}
