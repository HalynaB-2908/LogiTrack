using LogiTrack.WebApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/integration/shipments")]
    [Produces("application/json")]
    public class IntegrationShipmentsController : ControllerBase
    {
        private readonly LogiTrackDbContext _db;

        public IntegrationShipmentsController(LogiTrackDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllForIntegration()
        {
            if (!Request.Headers.TryGetValue("X-API-Key", out var providedKey))
                return Unauthorized("Missing API key.");

            var keyEntity = await _db.ApiKeys
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.Key == providedKey && k.IsActive);

            if (keyEntity == null)
                return Unauthorized("Invalid or inactive API key.");

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

            return Ok(new
            {
                IntegrationKeyName = keyEntity.Name,
                Count = shipments.Count,
                Data = shipments
            });
        }
    }
}

