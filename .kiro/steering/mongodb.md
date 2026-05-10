# Convenciones MongoDB — Kelas

## Referencia al DER

El modelo de datos completo está documentado en `#[[file:Analisis/DER-mongodb.md]]`.

## Colecciones

| Colección | Descripción |
|---|---|
| `rawMaterials` | Materias primas |
| `products` | Productos terminados (con receta embebida) |
| `stock` | Saldo actual de stock (MP y PT) |
| `stockMovements` | Historial de movimientos de stock (inmutable) |
| `rawMaterialPrices` | Historial de precios de MP (auditoría) |
| `purchases` | Compras de materia prima |
| `productionBatches` | Tandas de producción |
| `sales` | Ventas |
| `expenses` | Gastos operativos |
| `cashAccounts` | Cuentas de caja |
| `cashMovements` | Movimientos de caja (inmutable) |

## Cómo Crear un Repositorio

Cada colección tiene su repositorio. Patrón base:

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(string id);
    Task<List<T>> GetAllAsync();
    Task<T> CreateAsync(T entity);
    Task UpdateAsync(string id, T entity);
    Task DeleteAsync(string id);
}

public class RawMaterialRepository : IRawMaterialRepository
{
    private readonly IMongoCollection<RawMaterial> _collection;

    public RawMaterialRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<RawMaterial>("rawMaterials");
    }

    public async Task<RawMaterial?> GetByIdAsync(string id)
    {
        var objectId = ObjectId.Parse(id);
        return await _collection.Find(x => x.Id == objectId).FirstOrDefaultAsync();
    }

    public async Task<RawMaterial> CreateAsync(RawMaterial entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(string id, RawMaterial entity)
    {
        var objectId = ObjectId.Parse(id);
        entity.UpdatedAt = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(x => x.Id == objectId, entity);
    }
}
```

## Manejo de ObjectId

- En las entidades (Models/Entities), usar `ObjectId` nativo de MongoDB.
- En los DTOs (Requests/Responses), usar `string`.
- Conversión en el servicio o con atributos de serialización.

```csharp
// Entity
public class RawMaterial
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("unit")]
    public string Unit { get; set; } = string.Empty;

    [BsonElement("lastPricePerUnit")]
    public decimal LastPricePerUnit { get; set; }

    [BsonElement("minStock")]
    public decimal MinStock { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
}

// Response DTO
public class RawMaterialResponse
{
    public string Id { get; set; } = string.Empty;  // ObjectId.ToString()
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal LastPricePerUnit { get; set; }
    public decimal MinStock { get; set; }
    public decimal CurrentQuantity { get; set; }  // Viene de Stock
    public string Status { get; set; } = string.Empty;  // Calculado
}
```

## Naming de Colecciones

- **camelCase plural**: `rawMaterials`, `cashMovements`, `productionBatches`.
- Los nombres de colección se definen como constantes o en la configuración del repositorio.
- Los campos dentro de los documentos también son **camelCase**: `lastPricePerUnit`, `currentQuantity`.

## Índices

Los índices se crean al iniciar la aplicación (en un servicio de inicialización o en el repositorio):

```csharp
public class DatabaseInitializer
{
    private readonly IMongoDatabase _database;

    public async Task InitializeAsync()
    {
        // rawMaterials
        var rawMaterials = _database.GetCollection<RawMaterial>("rawMaterials");
        await rawMaterials.Indexes.CreateOneAsync(
            new CreateIndexModel<RawMaterial>(
                Builders<RawMaterial>.IndexKeys.Ascending(x => x.Name),
                new CreateIndexOptions { Unique = true }));

        // stock
        var stock = _database.GetCollection<Stock>("stock");
        await stock.Indexes.CreateOneAsync(
            new CreateIndexModel<Stock>(
                Builders<Stock>.IndexKeys
                    .Ascending(x => x.ItemType)
                    .Ascending(x => x.ItemId),
                new CreateIndexOptions { Unique = true }));

        // ... más índices según DER
    }
}
```

## Documentos Inmutables

Las colecciones `stockMovements` y `cashMovements` son **inmutables**:
- No exponer endpoints de PUT/PATCH/DELETE para estas colecciones.
- Solo INSERT.
- Los repositorios de estas colecciones no tienen métodos `Update` ni `Delete`.

## Queries con Filtros

Para queries con filtros opcionales, construir el filtro dinámicamente:

```csharp
public async Task<List<StockMovement>> GetByFiltersAsync(StockMovementFilter filter)
{
    var builder = Builders<StockMovement>.Filter;
    var filters = new List<FilterDefinition<StockMovement>>();

    if (!string.IsNullOrEmpty(filter.ItemType))
        filters.Add(builder.Eq(x => x.ItemType, filter.ItemType));

    if (!string.IsNullOrEmpty(filter.ItemId))
        filters.Add(builder.Eq(x => x.ItemId, ObjectId.Parse(filter.ItemId)));

    if (filter.DateFrom.HasValue)
        filters.Add(builder.Gte(x => x.Date, filter.DateFrom.Value));

    if (filter.DateTo.HasValue)
        filters.Add(builder.Lte(x => x.Date, filter.DateTo.Value));

    var combinedFilter = filters.Any()
        ? builder.And(filters)
        : builder.Empty;

    return await _collection
        .Find(combinedFilter)
        .Sort(Builders<StockMovement>.Sort.Descending(x => x.Date))
        .ToListAsync();
}
```

## Aggregations

Para cálculos como dashboard o KPIs, usar el pipeline de aggregation:

```csharp
public async Task<List<CategoryTotal>> GetExpensesByCategoryAsync(DateTime from, DateTime to)
{
    var pipeline = new[]
    {
        new BsonDocument("$match", new BsonDocument
        {
            { "date", new BsonDocument { { "$gte", from }, { "$lte", to } } }
        }),
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", "$category" },
            { "total", new BsonDocument("$sum", "$amount") },
            { "count", new BsonDocument("$sum", 1) }
        }),
        new BsonDocument("$sort", new BsonDocument("total", -1))
    };

    return await _collection.Aggregate<CategoryTotal>(pipeline).ToListAsync();
}
```

## Queries por Lote con `$in` (OBLIGATORIO)

**Nunca hacer un loop de queries individuales cuando se necesitan múltiples documentos por id.** Usar siempre una sola query con el operador `$in`.

### ❌ MAL — N queries al repositorio (una por id)

```csharp
// NUNCA hacer esto
foreach (var item in recipe)
{
    var rawMaterial = await _rawMaterialRepository.GetByIdAsync(item.RawMaterialId.ToString());
    // ...
}
```

### ✅ BIEN — Una sola query con $in

```csharp
// Agregar al repositorio
public async Task<List<RawMaterial>> GetByIdsAsync(IEnumerable<ObjectId> ids)
{
    var filter = Builders<RawMaterial>.Filter.In(x => x.Id, ids);
    return await _collection.Find(filter).ToListAsync();
}

// Usar en el servicio
var rawMaterialIds = recipe.Select(x => x.RawMaterialId).ToList();
var rawMaterials = await _rawMaterialRepository.GetByIdsAsync(rawMaterialIds);
var rawMaterialsDict = rawMaterials.ToDictionary(x => x.Id);

// Luego iterar en memoria sobre el diccionario
var estimatedCost = recipe.Sum(item =>
    rawMaterialsDict.TryGetValue(item.RawMaterialId, out var rm)
        ? item.Quantity * rm.LastPricePerUnit
        : 0m);
```

Este patrón aplica a cualquier situación donde se necesiten múltiples documentos por id: recetas, lotes de producción, movimientos de stock, etc. La regla es: **una sola ida a la base de datos, luego operar en memoria con un diccionario**.

## Transacciones

Para operaciones que afectan múltiples colecciones (ej: registrar compra → actualizar stock + crear movimiento + actualizar precio), usar transacciones de MongoDB:

```csharp
using var session = await _client.StartSessionAsync();
session.StartTransaction();
try
{
    // Múltiples operaciones...
    await session.CommitTransactionAsync();
}
catch
{
    await session.AbortTransactionAsync();
    throw;
}
```

**Nota**: Las transacciones requieren replica set. En desarrollo local con docker-compose, configurar MongoDB como replica set de un solo nodo.
