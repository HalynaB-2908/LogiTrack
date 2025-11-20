using LogiTrack.WebApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LogiTrack.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/integration/shipments")]
    [Produces("application/json")]
    public class IntegrationShipmentsController : ControllerBase
    {
        private readonly LogiTrackDbContext _db;
        private readonly ILogger<IntegrationShipmentsController> _logger;

        public IntegrationShipmentsController(LogiTrackDbContext db, ILogger<IntegrationShipmentsController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllForIntegration()
        {
            if (!Request.Headers.TryGetValue("X-API-Key", out var providedKey))
            {
                _logger.LogWarning("Integration access denied: missing X-API-Key header.");
                return Unauthorized("Missing API key.");
            }

            var keyEntity = await _db.ApiKeys
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.Key == providedKey && k.IsActive);

            if (keyEntity == null)
            {
                _logger.LogWarning("Integration access denied: invalid or inactive API key.");
                return Unauthorized("Invalid or inactive API key.");
            }

            var shipments = await _db.Shipments
                .AsNoTracking()
                .Select(s => new
                {
                    s.Id,
                    s.Reference,
                    Status = s.Status.ToString(),
                    Customer = s.Customer != null ? s.Customer.Name : null,
                    Vehicle = s.Vehicle != null ? s.Vehicle.PlateNumber : null,
                    s.DistanceKm,
                    s.WeightKg,
                    s.Price,
                    s.CreatedUtc
                })
                .ToListAsync();

            _logger.LogInformation(
                "Integration shipments requested by API key Id={ApiKeyId}, Name={ApiKeyName}. Returned {ShipmentCount} records.",
                keyEntity.Id,
                keyEntity.Name,
                shipments.Count
            );

            return Ok(new
            {
                IntegrationKeyName = keyEntity.Name,
                Count = shipments.Count,
                Data = shipments
            });
        }
    }
}
