using System.Text;
using Kelas.Api.Middleware;
using Kelas.Domain.Configuration;
using Kelas.Domain.Interfaces.Repositories;
using Kelas.Domain.Interfaces.Services;
using Kelas.IoC.Resolver;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Kelas services (MongoDB, Repositories, Services)
builder.Services.AddKelasServices(builder.Configuration);

// 2. JWT Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("La configuración 'Jwt' es requerida.");

if (string.IsNullOrWhiteSpace(jwtSettings.Secret))
    throw new InvalidOperationException("La configuración 'Jwt:Secret' es requerida y no puede estar vacía.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// 3. CORS
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]?
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        ?? ["http://localhost:3000"];

    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 4. Controllers
builder.Services.AddControllers();

var app = builder.Build();

// 5. Middleware pipeline
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 6. Seed de datos iniciales e inicialización de índices
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<ICashAccountSeeder>();
    await seeder.SeedAsync();

    // Inicializar índices de materias primas y stock
    var rawMaterialRepo = scope.ServiceProvider.GetRequiredService<IRawMaterialRepository>();
    await rawMaterialRepo.EnsureIndexesAsync();

    var stockRepo = scope.ServiceProvider.GetRequiredService<IStockRepository>();
    await stockRepo.EnsureIndexesAsync();

    // Inicializar índices de compras y movimientos
    var purchaseRepo = scope.ServiceProvider.GetRequiredService<IPurchaseRepository>();
    await purchaseRepo.EnsureIndexesAsync();

    var stockMovementRepo = scope.ServiceProvider.GetRequiredService<IStockMovementRepository>();
    await stockMovementRepo.EnsureIndexesAsync();

    var cashMovementRepo = scope.ServiceProvider.GetRequiredService<ICashMovementRepository>();
    await cashMovementRepo.EnsureIndexesAsync();

    var rawMaterialPriceRepo = scope.ServiceProvider.GetRequiredService<IRawMaterialPriceRepository>();
    await rawMaterialPriceRepo.EnsureIndexesAsync();

    var productRepo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
    await productRepo.EnsureIndexesAsync();

    var productionBatchRepo = scope.ServiceProvider.GetRequiredService<IProductionBatchRepository>();
    await productionBatchRepo.EnsureIndexesAsync();
}

app.Run();
