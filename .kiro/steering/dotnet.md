---
inclusion: fileMatch
fileMatchPattern: ['**/kelas-backend/**/*.cs', '**/kelas-backend/**/*.csproj', '**/kelas-backend/**/*.sln']
---

# .NET Backend Conventions — Kelas

## Architecture

.NET 8 Web API in `kelas-backend/` with strict layered architecture across 5 projects. Nullable reference types and implicit usings are enabled globally.

### Project Dependency Rules

```
Kelas.Api → Kelas.IoC.Resolver, Kelas.Domain
Kelas.IoC.Resolver → Kelas.Services, Kelas.Repositories, Kelas.Domain
Kelas.Services → Kelas.Domain
Kelas.Repositories → Kelas.Domain, MongoDB.Driver
Kelas.Domain → (no project references)
```

| Project | Responsibility | Forbidden |
|---------|---------------|-----------|
| `Kelas.Domain` | Entities, interfaces, DTOs, exceptions, config classes | No project refs, no MongoDB.Driver |
| `Kelas.Repositories` | MongoDB data access implementations | No business logic |
| `Kelas.Services` | Business logic, validation, entity↔DTO mapping | No direct MongoDB access |
| `Kelas.IoC.Resolver` | DI wiring via `AddKelasServices()` extension | No business logic |
| `Kelas.Api` | Controllers, middleware, Program.cs | No direct repository references |

## File Placement

When creating new code, place files exactly as follows:

| Artifact | Location |
|----------|----------|
| Entity class | `Kelas.Domain/Entities/{Name}.cs` |
| Request DTO | `Kelas.Domain/Models/Requests/{Name}Request.cs` |
| Response DTO | `Kelas.Domain/Models/Responses/{Name}Response.cs` |
| Custom exception | `Kelas.Domain/Exceptions/{Name}Exception.cs` |
| Repository interface | `Kelas.Domain/Interfaces/Repositories/I{Name}Repository.cs` |
| Service interface | `Kelas.Domain/Interfaces/Services/I{Name}Service.cs` |
| Repository impl | `Kelas.Repositories/{Name}Repository.cs` |
| Service impl | `Kelas.Services/{Name}Service.cs` |
| Controller | `Kelas.Api/Controllers/{Name}Controller.cs` |
| DI registration | `Kelas.IoC.Resolver/DependencyResolver.cs` (add to existing) |

## Controller Pattern

Controllers are thin routing layers. No business logic, no direct data access.

```csharp
using Kelas.Domain.Interfaces.Services;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kelas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExampleController : ControllerBase
{
    private readonly IExampleService _service;

    public ExampleController(IExampleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<ExampleResponse>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExampleResponse>> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ExampleResponse>> Create([FromBody] CreateExampleRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
```

- Only `AuthController` and `HealthController` omit `[Authorize]`.
- Return `Ok()`, `CreatedAtAction()`, or let exceptions propagate to middleware.

## Service Pattern

Services own all business logic. They map between entities and DTOs manually.

```csharp
using Kelas.Domain.Exceptions;
using Kelas.Domain.Interfaces.Repositories;
using Kelas.Domain.Interfaces.Services;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;

namespace Kelas.Services;

public class ExampleService : IExampleService
{
    private readonly IExampleRepository _repository;

    public ExampleService(IExampleRepository repository)
    {
        _repository = repository;
    }

    public async Task<ExampleResponse> GetByIdAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Example", id);

        return MapToResponse(entity);
    }

    public async Task<ExampleResponse> CreateAsync(CreateExampleRequest request)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new BusinessException("El nombre es obligatorio.");

        // Map to entity, persist, return response
        var entity = MapToEntity(request);
        var created = await _repository.CreateAsync(entity);
        return MapToResponse(created);
    }

    private static ExampleResponse MapToResponse(ExampleEntity entity) => new()
    {
        Id = entity.Id.ToString(),
        Name = entity.Name
    };
}
```

- Throw `BusinessException` for validation failures (→ HTTP 400).
- Throw `NotFoundException` for missing entities (→ HTTP 404).
- `NotFoundException` constructor: `new NotFoundException("EntityName", id)` produces message `"{EntityName} con id '{id}' no encontrado."`.

## Repository Pattern

Repositories are the only layer that touches MongoDB.

```csharp
using Kelas.Domain.Entities;
using Kelas.Domain.Interfaces.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kelas.Repositories;

public class ExampleRepository : IExampleRepository
{
    private readonly IMongoCollection<ExampleEntity> _collection;

    public ExampleRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<ExampleEntity>("examples");
    }

    public async Task<ExampleEntity?> GetByIdAsync(string id)
    {
        var objectId = ObjectId.Parse(id);
        return await _collection.Find(x => x.Id == objectId).FirstOrDefaultAsync();
    }

    public async Task<ExampleEntity> CreateAsync(ExampleEntity entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(string id, ExampleEntity entity)
    {
        var objectId = ObjectId.Parse(id);
        entity.UpdatedAt = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(x => x.Id == objectId, entity);
    }
}
```

- Collection names: camelCase plural (e.g., `rawMaterials`, `stockMovements`).
- Use `Builders<T>.Filter` for dynamic filter construction on optional parameters.
- Immutable collections (`stockMovements`, `cashMovements`): expose only insert methods, never update/delete.

## Entity Pattern

```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kelas.Domain.Entities;

public class ExampleEntity
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
}
```

- `[BsonId]` on `ObjectId Id`.
- `[BsonElement("camelCaseName")]` on every persisted property.
- Always include `CreatedAt` and `UpdatedAt`.

## DTO Pattern

```csharp
namespace Kelas.Domain.Models.Responses;

public class ExampleResponse
{
    public string Id { get; set; } = string.Empty;  // ObjectId.ToString()
    public string Name { get; set; } = string.Empty;
}
```

- IDs are `string` in DTOs (converted from `ObjectId` in service layer).
- No `[BsonElement]` attributes on DTOs.

## DI Registration

All registrations go in `Kelas.IoC.Resolver/DependencyResolver.cs`:

```csharp
private static void AddRepositories(IServiceCollection services)
{
    services.AddScoped<IExampleRepository, ExampleRepository>();
}

private static void AddServices(IServiceCollection services)
{
    services.AddScoped<IExampleService, ExampleService>();
}
```

- `IMongoClient`: Singleton.
- `IMongoDatabase`: Scoped.
- All repositories and services: Scoped.

## Error Handling Middleware

Already configured in `Program.cs`. Exception mapping:

| Exception | HTTP Status | Response Body |
|-----------|-------------|---------------|
| `BusinessException` | 400 | `{ "error": "<message>" }` |
| `NotFoundException` | 404 | `{ "error": "<message>" }` |
| Unhandled | 500 | `{ "error": "Error interno del servidor" }` |

Do not add try/catch in controllers — let exceptions propagate to middleware.

## Program.cs Pipeline

The middleware pipeline order in `Program.cs`:

```csharp
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors();
// Authentication/Authorization middleware goes here when added
app.MapControllers();
```

CORS allows `http://localhost:3000` in development.

## Naming Conventions

| Element | Rule | Example |
|---------|------|---------|
| Classes/Methods/Properties | PascalCase | `RawMaterialService`, `GetAllAsync` |
| Async methods | Always suffix with `Async` | `CreateAsync`, `GetByIdAsync` |
| Interfaces | Prefix with `I` | `IRawMaterialRepository` |
| Local variables/parameters | camelCase | `rawMaterial`, `filterBuilder` |
| Files | Match class name exactly | `RawMaterialService.cs` |
| Namespaces | Mirror folder path | `Kelas.Domain.Entities` |
| MongoDB collections | camelCase plural | `rawMaterials`, `cashMovements` |
| MongoDB fields | camelCase | `lastPricePerUnit`, `currentQuantity` |

## Configuration

Settings in `appsettings.json`, overridable via environment variables using `Section__Key` format:

| Section | Keys | Env Override |
|---------|------|--------------|
| `MongoDb` | `ConnectionString`, `DatabaseName` | `MongoDb__ConnectionString` |
| `Auth` | `Email`, `Password` | `Auth__Email`, `Auth__Password` |
| `Jwt` | `Secret`, `ExpirationHours` | `Jwt__Secret`, `Jwt__ExpirationHours` |

## Transactions

Use MongoDB sessions for operations spanning multiple collections:

```csharp
using var session = await _client.StartSessionAsync();
session.StartTransaction();
try
{
    // multiple collection operations with session parameter
    await session.CommitTransactionAsync();
}
catch
{
    await session.AbortTransactionAsync();
    throw;
}
```

Requires replica set (configured in docker-compose for local dev).

## Prohibited Patterns

- No AutoMapper — map manually in services.
- No MediatR or CQRS — direct service calls.
- No FluentValidation — validate in service methods.
- No repository references from `Kelas.Api`.
- No `MongoDB.Driver` usage in `Kelas.Domain`.
- No business logic in controllers or repositories.
- No update/delete on immutable collections (`stockMovements`, `cashMovements`).

## New Feature Checklist

When implementing a new module, create files in this order:

1. Entity → `Kelas.Domain/Entities/`
2. Request DTO(s) → `Kelas.Domain/Models/Requests/`
3. Response DTO(s) → `Kelas.Domain/Models/Responses/`
4. Repository interface → `Kelas.Domain/Interfaces/Repositories/`
5. Service interface → `Kelas.Domain/Interfaces/Services/`
6. Repository implementation → `Kelas.Repositories/`
7. Service implementation → `Kelas.Services/`
8. Register in DI → `Kelas.IoC.Resolver/DependencyResolver.cs`
9. Controller → `Kelas.Api/Controllers/`
