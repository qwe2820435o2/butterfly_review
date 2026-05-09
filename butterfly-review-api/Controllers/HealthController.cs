using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;

namespace tennis_wave_api.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly IMongoClient? _mongoClient;

    public HealthController(IMongoClient? mongoClient = null)
    {
        _mongoClient = mongoClient;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

    [HttpGet("mongo")]
    public async Task<IActionResult> Mongo()
    {
        if (_mongoClient == null)
        {
            return Ok(new { connected = false, reason = "MongoDB not configured" });
        }

        try
        {
            // Attempt a simple ping command against the admin database
            var admin = _mongoClient.GetDatabase("admin");
            var command = new BsonDocument("ping", 1);
            await admin.RunCommandAsync<BsonDocument>(command);
            return Ok(new { connected = true });
        }
        catch (Exception ex)
        {
            return Ok(new { connected = false, error = ex.Message });
        }
    }
} 