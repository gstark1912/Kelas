using Kelas.Domain.Configuration;
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

        // Repositories
        AddRepositories(services);

        // Services
        AddServices(services);

        return services;
    }

    private static void AddRepositories(IServiceCollection services)
    {
        // Repositories will be registered here as they are created
    }

    private static void AddServices(IServiceCollection services)
    {
        // Services will be registered here as they are created
    }
}
