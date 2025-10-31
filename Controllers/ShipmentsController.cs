using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using LogiTrack.WebApi.Contracts;
using LogiTrack.WebApi.Options;
using LogiTrack.WebApi.Services;
using LogiTrack.WebApi.Models;
using LogiTrack.WebApi.Data;

namespace LogiTrack.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin,User")]
    public class ShipmentsController : ControllerBase
    {
        private readonly LogisticsOptions _options;
        private readonly IDeliveryTimeService _delivery;
        private readonly LogiTrackDbContext _db;

        public ShipmentsController(
            IOptions<LogisticsOptions> options,
            IDeliveryTimeService delivery,
            LogiTrackDbContext db)
        {
            _options = options.Value;
            _delivery = delivery;
            _db = db;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.Shipments
                .Include(s => s.Customer)
                .Include(s => s.Vehicle)
                .OrderBy(s => s.Id)
                .Select(s => new
                {
                    id = s.Id,
                    reference = s.Reference,
                    status = s.Status,
                    distanceKm = s.DistanceKm,
                    weightKg = s.WeightKg,
                    createdUtc = s.CreatedUtc,
                    customerId = s.CustomerId,
                    customerName = s.Customer != null ? s.Customer.Name : null,
                    vehicleId = s.VehicleId,
                    vehiclePlate = s.Vehicle != null ? s.Vehicle.PlateNumber : null,
                    estimatedPrice = Math.Round(
                        _options.BasePricePerKm * s.DistanceKm
                        + _options.WeightPricePerKg * s.WeightKg,
                        2
                    ),
                    currency = _options.Currency
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            if (id <= 0) return BadRequest("Id must be greater than 0.");

            var s = await _db.Shipments
                .Include(x => x.Customer)
                .Include(x => x.Vehicle)
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    id = x.Id,
                    reference = x.Reference,
                    status = x.Status,
                    distanceKm = x.DistanceKm,
                    weightKg = x.WeightKg,
                    createdUtc = x.CreatedUtc,
                    customerId = x.CustomerId,
                    customerName = x.Customer != null ? x.Customer.Name : null,
                    vehicleId = x.VehicleId,
                    vehiclePlate = x.Vehicle != null ? x.Vehicle.PlateNumber : null,
                    estimatedPrice = Math.Round(
                        _options.BasePricePerKm * x.DistanceKm
                        + _options.WeightPricePerKg * x.WeightKg,
                        2
                    ),
                    currency = _options.Currency
                })
                .FirstOrDefaultAsync();

            if (s == null) return NotFound($"Shipment with id {id} not found.");
            return Ok(s);
        }

        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] string? status)
        {
            if (string.IsNullOrWhiteSpace(q) && string.IsNullOrWhiteSpace(status))
                return BadRequest("At least one search parameter must be provided.");

            ShipmentStatus? statusEnum = null;
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<ShipmentStatus>(status, true, out var parsed))
                    statusEnum = parsed;
                else
                    return BadRequest(
                        $"Unknown status '{status}'. Allowed: {string.Join(", ", Enum.GetNames(typeof(ShipmentStatus)))}"
                    );
            }

            var query = _db.Shipments
                .Include(s => s.Customer)
                .Include(s => s.Vehicle)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(s => s.Reference.Contains(q));

            if (statusEnum.HasValue)
                query = query.Where(s => s.Status == statusEnum.Value);

            var results = await query
                .OrderBy(s => s.Id)
                .Select(s => new
                {
                    id = s.Id,
                    reference = s.Reference,
                    status = s.Status,
                    distanceKm = s.DistanceKm,
                    weightKg = s.WeightKg,
                    createdUtc = s.CreatedUtc,
                    customerId = s.CustomerId,
                    customerName = s.Customer != null ? s.Customer.Name : null,
                    vehicleId = s.VehicleId,
                    vehiclePlate = s.Vehicle != null ? s.Vehicle.PlateNumber : null,
                    estimatedPrice = Math.Round(
                        _options.BasePricePerKm * s.DistanceKm
                        + _options.WeightPricePerKg * s.WeightKg,
                        2
                    ),
                    currency = _options.Currency
                })
                .ToListAsync();

            return Ok(results);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateShipmentDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Reference))
                return BadRequest("Reference is required.");
            if (request.DistanceKm <= 0)
                return BadRequest("DistanceKm must be greater than 0.");

            var defaultStatus = Enum.TryParse<ShipmentStatus>(
                _options.DefaultShipmentStatus,
                true,
                out var parsedStatus
            )
                ? parsedStatus
                : ShipmentStatus.Planned;

            var entity = new Shipment
            {
                Reference = request.Reference.Trim(),
                Status = defaultStatus,
                DistanceKm = request.DistanceKm,
                WeightKg = request.WeightKg,
                CustomerId = request.CustomerId,
                VehicleId = request.VehicleId,
                CreatedUtc = DateTime.UtcNow
            };

            _db.Shipments.Add(entity);
            await _db.SaveChangesAsync();

            var etaHours = _delivery.Estimate(entity.DistanceKm);
            var estimatedPrice = Math.Round(
                _options.BasePricePerKm * entity.DistanceKm
                + _options.WeightPricePerKg * entity.WeightKg,
                2
            );

            return CreatedAtAction(
                nameof(GetById),
                new { id = entity.Id },
                new
                {
                    id = entity.Id,
                    entity.Reference,
                    entity.Status,
                    distanceKm = entity.DistanceKm,
                    weightKg = entity.WeightKg,
                    createdUtc = entity.CreatedUtc,
                    customerId = entity.CustomerId,
                    vehicleId = entity.VehicleId,
                    estimatedTimeHours = Math.Round(etaHours, 2),
                    estimatedArrival = DateTime
                        .Now
                        .AddHours(etaHours)
                        .ToString("yyyy-MM-dd HH:mm"),
                    estimatedPrice,
                    currency = _options.Currency
                }
            );
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CreateShipmentDto request)
        {
            if (id <= 0) return BadRequest("Id must be greater than 0.");
            if (request == null || string.IsNullOrWhiteSpace(request.Reference))
                return BadRequest("Reference is required.");
            if (request.DistanceKm <= 0)
                return BadRequest("DistanceKm must be greater than 0.");

            var entity = await _db.Shipments.FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null)
                return NotFound($"Shipment with id {id} not found.");

            entity.Reference = request.Reference.Trim();
            entity.DistanceKm = request.DistanceKm;
            entity.WeightKg = request.WeightKg;
            entity.CustomerId = request.CustomerId;
            entity.VehicleId = request.VehicleId;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if (id <= 0) return BadRequest("Id must be greater than 0.");

            var entity = await _db.Shipments.FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null)
                return NotFound($"Shipment with id {id} not found.");

            _db.Shipments.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("export")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Export(
            [FromServices] IOptions<StorageOptions> storage,
            [FromServices] IWebHostEnvironment env)
        {
            var path = storage.Value.ShipmentsFilePath;
            if (!Path.IsPathRooted(path))
                path = Path.Combine(env.ContentRootPath, path);

            if (!System.IO.File.Exists(path))
                return NotFound("Data file not found.");

            var bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/json", "shipments.json");
        }
    }
}
