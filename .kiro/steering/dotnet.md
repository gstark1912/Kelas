# Convenciones .NET — Kelas Backend

## Arquitectura Multi-Proyecto

El backend vive en `kelas-backend/` y está organizado en 5 proyectos con separación de responsabilidades:

```
kelas-backend/
├── Kelas.sln
├── scripts/
│   └── mongo-init.js
├── Kelas.Api/                    # Web API — Entry point
│   ├── Controllers/              # Controllers livianos, solo routing
│   ├── Middleware/               # Error handling, logging
│   ├── Properties/
│   ├── Program.cs                # Configuración de host, CORS, pipeline
│   ├── appsettings.json
│   ├── Dockerfile
│   └── railway.toml
├── Kelas.Services/               # Lógica de negocio
│   └── (implementaciones de servicios)
├── Kelas.Repositories/           # Acceso a datos (MongoDB)
│   └── (implementaciones de repositorios)
├── Kelas.Domain/                 # Núcleo del dominio — sin dependencias
│   ├── Configuration/            # MongoDbSettings, etc.
│   ├── Entities/                 # Modelos de MongoDB
│   ├── Exceptions/               # BusinessException, NotFoundException
│   ├── Interfaces/
│   │   ├── Repositories/         # Interfaces de repositorios
│   │   └── Services/             # Interfaces de servicios
│   └── Models/
│       ├── Requests/             # DTOs de entrada
│       └── Responses/            # DTOs de salida
└── Kelas.IoC.Resolver/           # Composición de dependencias
    └── DependencyResolver.cs     # Extension method AddKelasServices()
```

## Grafo de Dependencias

```
Kelas.Api → Kelas.IoC.Resolver → Kelas.Services → Kelas.Domain
         → Kelas.Domain                          
                               → Kelas.Repositories → Kelas.Domain
```

- **Kelas.Domain**: Sin referencias a otros proyectos Kelas. Solo define contratos.
- **Kelas.Repositories**: Referencia a Domain. Implementa las interfaces de repositorios.
- **Kelas.Services**: Referencia a Domain. Implementa las interfaces de servicios.
- **Kelas.IoC.Resolver**: Referencia a Domain, Repositories y Services. Registra todo en DI.
- **Kelas.Api**: Referencia a Domain (para DTOs/excepciones) y IoC.Resolver (para DI).

## Responsabilidades por Proyecto

### Kelas.Domain

- Entidades (mapean a documentos MongoDB)
- Interfaces de repositorios (`IRawMaterialRepository`, etc.)
- Interfaces de servicios (`IRawMaterialService`, etc.)
- DTOs de Request y Response
- Excepciones de negocio (`BusinessException`, `NotFoundException`)
- Clases de configuración (`MongoDbSettings`)
- **NO tiene dependencias** a otros proyectos Kelas ni a MongoDB.Driver

### Kelas.Repositories

- Implementaciones de repositorios
- Acceso directo a MongoDB (único proyecto que hace queries)
- NuGet: `MongoDB.Driver`

### Kelas.Services

- Implementaciones de servicios
- Lógica de negocio, validaciones, orquestación
- Mapeo entre Entities y DTOs
- **NO accede a MongoDB directamente** — usa interfaces de repositorios

### Kelas.IoC.Resolver

- `DependencyResolver.cs` con método `AddKelasServices(IServiceCollection, IConfiguration)`
- Registra MongoDB (IMongoClient Singleton, IMongoDatabase Scoped)
- Registra repositorios y servicios
- NuGet: `MongoDB.Driver`, `Microsoft.Extensions.DependencyInjection.Abstractions`, `Microsoft.Extensions.Options.ConfigurationExtensions`, `Microsoft.Extensions.Configuration.Abstractions`

### Kelas.Api

- Controllers (livianos, solo routing)
- Middleware (error handling)
- Program.cs (configuración del host)
- Archivos de configuración y deploy
- NuGet: `MongoDB.Driver` (para HealthController)

## Patrones Obligatorios

### Repository Pattern

- Los repositorios son los **únicos** que interactúan con MongoDB.
- Cada colección tiene su propio repositorio.
- Interfaz en `Kelas.Domain/Interfaces/Repositories/`, implementación en `Kelas.Repositories/`.

```csharp
// Kelas.Domain/Interfaces/Repositories/IRawMaterialRepository.cs
namespace Kelas.Domain.Interfaces.Repositories;

public interface IRawMaterialRepository
{
    Task<RawMaterial?> GetByIdAsync(string id);
    Task<List<RawMaterial>> GetAllAsync(RawMaterialFilter filter);
    Task<RawMaterial> CreateAsync(RawMaterial entity);
    Task UpdateAsync(string id, RawMaterial entity);
}

// Kelas.Repositories/RawMaterialRepository.cs
namespace Kelas.Repositories;

public class RawMaterialRepository : IRawMaterialRepository
{
    private readonly IMongoCollection<RawMaterial> _collection;

    public RawMaterialRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<RawMaterial>("rawMaterials");
    }
    // ...
}
```

### Service Pattern

- Interfaz en `Kelas.Domain/Interfaces/Services/`, implementación en `Kelas.Services/`.
- Contienen toda la lógica de negocio.
- Orquestan llamadas a múltiples repositorios.
- Validan reglas de negocio.
- Mapean entre Entities y DTOs.

```csharp
// Kelas.Domain/Interfaces/Services/IRawMaterialService.cs
namespace Kelas.Domain.Interfaces.Services;

public interface IRawMaterialService
{
    Task<List<RawMaterialResponse>> GetAllAsync(RawMaterialFilter filter);
    Task<RawMaterialResponse> CreateAsync(CreateRawMaterialRequest request);
}

// Kelas.Services/RawMaterialService.cs
namespace Kelas.Services;

public class RawMaterialService : IRawMaterialService
{
    private readonly IRawMaterialRepository _rawMaterialRepo;
    private readonly IStockRepository _stockRepo;

    public async Task<RawMaterialResponse> CreateAsync(CreateRawMaterialRequest request)
    {
        var existing = await _rawMaterialRepo.GetByNameAsync(request.Name);
        if (existing != null)
            throw new BusinessException("Ya existe una materia prima con ese nombre.");

        var entity = new RawMaterial { Name = request.Name, Unit = request.Unit, MinStock = request.MinStock };
        var created = await _rawMaterialRepo.CreateAsync(entity);

        await _stockRepo.CreateAsync(new Stock
        {
            ItemType = "RawMaterial",
            ItemId = created.Id,
            CurrentQuantity = 0
        });

        return MapToResponse(created);
    }
}
```

### Dependency Injection (IoC.Resolver)

```csharp
// Kelas.IoC.Resolver/DependencyResolver.cs
namespace Kelas.IoC.Resolver;

public static class DependencyResolver
{
    public static IServiceCollection AddKelasServices(this IServiceCollection services, IConfiguration configuration)
    {
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

        AddRepositories(services);
        AddServices(services);

        return services;
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IRawMaterialRepository, RawMaterialRepository>();
        // ...
    }

    private static void AddServices(IServiceCollection services)
    {
        services.AddScoped<IRawMaterialService, RawMaterialService>();
        // ...
    }
}
```

### Controllers Livianos

- Sin lógica de negocio.
- Solo: recibir request → llamar servicio → retornar response.
- Todos con `[Authorize]` excepto AuthController y HealthController.

```csharp
// Kelas.Api/Controllers/RawMaterialsController.cs
namespace Kelas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RawMaterialsController : ControllerBase
{
    private readonly IRawMaterialService _service;

    public RawMaterialsController(IRawMaterialService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<RawMaterialResponse>>> GetAll([FromQuery] RawMaterialFilter filter)
    {
        var result = await _service.GetAllAsync(filter);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<RawMaterialResponse>> Create([FromBody] CreateRawMaterialRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
```

## Manejo de Errores

- Excepciones custom en `Kelas.Domain/Exceptions/`.
- Middleware global en `Kelas.Api/Middleware/`.

```csharp
// Kelas.Domain/Exceptions/BusinessException.cs
namespace Kelas.Domain.Exceptions;

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}

// Kelas.Domain/Exceptions/NotFoundException.cs
namespace Kelas.Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entity, string id)
        : base($"{entity} con id '{id}' no encontrado.") { }
}

// Kelas.Api/Middleware/ErrorHandlingMiddleware.cs
namespace Kelas.Api.Middleware;

public class ErrorHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (BusinessException ex)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { error = "Error interno del servidor" });
        }
    }
}
```

## Naming Conventions

- **Clases**: PascalCase (`RawMaterialService`, `PurchaseRepository`).
- **Métodos**: PascalCase + Async suffix (`GetAllAsync`, `CreateAsync`).
- **Propiedades**: PascalCase (`CurrentQuantity`, `LastPricePerUnit`).
- **Variables locales y parámetros**: camelCase (`rawMaterial`, `totalCost`).
- **Interfaces**: Prefijo `I` (`IRawMaterialService`).
- **Archivos**: Mismo nombre que la clase (`RawMaterialService.cs`).
- **Namespaces**: Siguen la estructura de proyecto (`Kelas.Domain.Entities`, `Kelas.Services`, `Kelas.Repositories`).
- **Colecciones MongoDB**: camelCase plural (`rawMaterials`, `cashMovements`).

## Configuración de MongoDB

```csharp
// Kelas.Domain/Configuration/MongoDbSettings.cs
namespace Kelas.Domain.Configuration;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}
```

La configuración se lee desde `appsettings.json` y puede sobreescribirse con variables de ambiente (`MongoDb__ConnectionString`, `MongoDb__DatabaseName`).

## Cómo Agregar una Nueva Feature

1. **Entidad** → `Kelas.Domain/Entities/NuevaEntidad.cs`
2. **Request/Response DTOs** → `Kelas.Domain/Models/Requests/` y `Kelas.Domain/Models/Responses/`
3. **Interfaz de repositorio** → `Kelas.Domain/Interfaces/Repositories/INuevaEntidadRepository.cs`
4. **Interfaz de servicio** → `Kelas.Domain/Interfaces/Services/INuevaEntidadService.cs`
5. **Implementación de repositorio** → `Kelas.Repositories/NuevaEntidadRepository.cs`
6. **Implementación de servicio** → `Kelas.Services/NuevaEntidadService.cs`
7. **Registrar en DI** → `Kelas.IoC.Resolver/DependencyResolver.cs` (AddRepositories + AddServices)
8. **Controller** → `Kelas.Api/Controllers/NuevaEntidadController.cs`

## Tests de Integración

- Usar MongoDB real (containerizado con Testcontainers o instancia de test).
- Cada test class limpia la base antes de ejecutar.
- Usar `WebApplicationFactory<Program>` para tests de endpoints.
- No mockear repositorios — el objetivo es probar el flujo completo.

```csharp
public class RawMaterialsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RawMaterialsTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestTokenHelper.GenerateToken());
    }

    [Fact]
    public async Task Create_ValidRawMaterial_ReturnsCreated()
    {
        var request = new { Name = "Cera de Soja", Unit = "gr", MinStock = 500 };
        var response = await _client.PostAsJsonAsync("/api/raw-materials", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<RawMaterialResponse>();
        body!.Name.Should().Be("Cera de Soja");
    }
}
```

## Reglas Generales

- Código simple, sin sobre-ingeniería.
- No usar AutoMapper — mapeos manuales en los servicios.
- No usar MediatR ni CQRS — llamadas directas servicio → repositorio.
- Validaciones simples en el servicio (no FluentValidation a menos que se justifique).
- Async/await en todo el stack.
- Usar `ObjectId` de MongoDB como string en los DTOs (serializar/deserializar).
- Respetar la separación de proyectos: nunca referenciar Repositories desde Api directamente.
