using Kelas.Domain.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kelas.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly IMongoDatabase _database;

    public HealthController(IMongoDatabase database)
    {
        _database = database;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var response = new HealthCheckResponse
        {
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _database.RunCommandAsync<BsonDocument>(
                new BsonDocumentCommand<BsonDocument>(new BsonDocument("ping", 1)));

            response.Status = "healthy";
            response.Services["mongodb"] = "connected";

            return Ok(response);
        }
        catch (Exception ex)
        {
            response.Status = "unhealthy";
            response.Services["mongodb"] = "disconnected";
            response.Error = ex.Message;

            return StatusCode(503, response);
        }
    }
}
