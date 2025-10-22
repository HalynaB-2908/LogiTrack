using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using LogiTrack.WebApi.Contracts;
using LogiTrack.WebApi.Options;
using LogiTrack.WebApi.Services;
using LogiTrack.WebApi.Repositories.Shipments;
using LogiTrack.WebApi.Models;
using LogiTrack.WebApi.Data;

namespace LogiTrack.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class ShipmentsController : ControllerBase
    {
        private readonly LogisticsOptions _options;
        private readonly IDeliveryTimeService _delivery;
        private readonly IShipmentsRepository _repository;
        private readonly LogiTrackDbContext _db;

        public ShipmentsController(
            IOptions<LogisticsOptions> options,
            IDeliveryTimeService delivery,
            IShipmentsRepository repository,
            LogiTrackDbContext db)
        {
            _options = options.Value;
            _delivery = delivery;
            _repository = repository;
            _db = db;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.Shipments.AsNoTracking().ToListAsync();
            return Ok(items);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            if (id <= 0) return BadRequest("Id must be greater than 0.");
            var shipment = await _repository.GetByIdAsync(id);
            if (shipment == null) return NotFound($"Shipment with id {id} not found.");
            return Ok(shipment);
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
                    return BadRequest($"Unknown status '{status}'. Allowed: {string.Join(", ", Enum.GetNames(typeof(ShipmentStatus)))}");
            }

            var results = await _repository.SearchAsync(q, statusEnum);
            return Ok(results);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateShipmentDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Reference))
                return BadRequest("Reference is required.");
            if (request.DistanceKm <= 0)
                return BadRequest("DistanceKm must be greater than 0.");

            var etaHours = _delivery.Estimate(request.DistanceKm);
            var estimatedPrice = _options.BasePricePerKm * request.DistanceKm
                               + _options.WeightPricePerKg * request.WeightKg;

            var defaultStatus = Enum.TryParse<ShipmentStatus>(_options.DefaultShipmentStatus, true, out var parsedStatus)
                ? parsedStatus
                : ShipmentStatus.Planned;

            var shipment = new Shipment
            {
                Reference = request.Reference,
                Status = defaultStatus,
                DistanceKm = request.DistanceKm,
                WeightKg = request.WeightKg,
                CreatedUtc = DateTime.UtcNow
            };

            var created = await _repository.CreateAsync(shipment);

            var response = new
            {
                created.Id,
                created.Reference,
                created.Status,
                EstimatedTimeHours = Math.Round(etaHours, 2),
                EstimatedArrival = DateTime.Now.AddHours(etaHours).ToString("yyyy-MM-dd HH:mm"),
                EstimatedPrice = Math.Round(estimatedPrice, 2),
                Currency = _options.Currency
            };

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
        }

        [HttpPut("{id:int}")]
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
            if (entity == null) return NotFound($"Shipment with id {id} not found.");

            entity.Reference = request.Reference;
            entity.DistanceKm = request.DistanceKm;
            entity.WeightKg = request.WeightKg;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if (id <= 0) return BadRequest("Id must be greater than 0.");

            var entity = await _db.Shipments.FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null) return NotFound($"Shipment with id {id} not found.");

            _db.Shipments.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("export")]
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
