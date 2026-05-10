using Kelas.Api.Middleware;
using Kelas.IoC.Resolver;

var builder = WebApplication.CreateBuilder(args);

// 1. Kelas services (MongoDB, Repositories, Services)
builder.Services.AddKelasServices(builder.Configuration);

// 2. CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 3. Controllers
builder.Services.AddControllers();

var app = builder.Build();

// 4. Middleware pipeline
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors();
app.MapControllers();

app.Run();
