using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using LogiTrack.WebApi.Contracts.Shipments;
using LogiTrack.WebApi.Models;
using LogiTrack.WebApi.Options;
using LogiTrack.WebApi.Services.Abstractions;

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
        private readonly IShipmentsRepository _shipments;
        private readonly IUnitOfWork _uow;

        public ShipmentsController(
            IOptions<LogisticsOptions> options,
            IDeliveryTimeService delivery,
            IShipmentsRepository shipments,
            IUnitOfWork uow)
        {
            _options = options.Value;
            _delivery = delivery;
            _shipments = shipments;
            _uow = uow;
        }

        // GET: api/v1/shipments
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var items = await _shipments.GetAllAsync(ct);

            var result = items
                .OrderBy(s => s.Id)
                .Select(s => new ShipmentResponseDto
                {
                    Id = s.Id,
                    Reference = s.Reference,
                    Status = s.Status,
                    DistanceKm = s.DistanceKm,
                    WeightKg = s.WeightKg,
                    CreatedUtc = s.CreatedUtc,
                    CustomerId = s.CustomerId,
                    CustomerName = s.Customer?.Name,
                    VehicleId = s.VehicleId,
                    VehiclePlate = s.Vehicle?.PlateNumber,
                    EstimatedPrice = Math.Round(
                        _options.BasePricePerKm * s.DistanceKm +
                        _options.WeightPricePerKg * s.WeightKg, 2),
                    Currency = _options.Currency ?? "USD"
                })
                .ToList();

            return Ok(result);
        }

        // GET: api/v1/shipments/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
        {
            if (id <= 0) return BadRequest("Id must be greater than 0.");

            var s = await _shipments.GetByIdAsync(id, ct);
            if (s == null) return NotFound($"Shipment with id {id} not found.");

            var dto = new ShipmentResponseDto
            {
                Id = s.Id,
                Reference = s.Reference,
                Status = s.Status,
                DistanceKm = s.DistanceKm,
                WeightKg = s.WeightKg,
                CreatedUtc = s.CreatedUtc,
                CustomerId = s.CustomerId,
                CustomerName = s.Customer?.Name,
                VehicleId = s.VehicleId,
                VehiclePlate = s.Vehicle?.PlateNumber,
                EstimatedPrice = Math.Round(
                    _options.BasePricePerKm * s.DistanceKm +
                    _options.WeightPricePerKg * s.WeightKg, 2),
                Currency = _options.Currency ?? "USD"
            };

            return Ok(dto);
        }

        // GET: api/v1/shipments/search?q=REF&status=Planned
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] string? status, CancellationToken ct)
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

            var items = await _shipments.SearchAsync(q, statusEnum, ct);

            var result = items
                .OrderBy(s => s.Id)
                .Select(s => new ShipmentResponseDto
                {
                    Id = s.Id,
                    Reference = s.Reference,
                    Status = s.Status,
                    DistanceKm = s.DistanceKm,
                    WeightKg = s.WeightKg,
                    CreatedUtc = s.CreatedUtc,
                    CustomerId = s.CustomerId,
                    CustomerName = s.Customer?.Name,
                    VehicleId = s.VehicleId,
                    VehiclePlate = s.Vehicle?.PlateNumber,
                    EstimatedPrice = Math.Round(
                        _options.BasePricePerKm * s.DistanceKm +
                        _options.WeightPricePerKg * s.WeightKg, 2),
                    Currency = _options.Currency ?? "USD"
                })
                .ToList();

            return Ok(result);
        }

        // POST: api/v1/shipments
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] ShipmentCreateUpdateDto dto, CancellationToken ct)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Reference))
                return BadRequest("Reference is required.");
            if (dto.DistanceKm <= 0)
                return BadRequest("DistanceKm must be greater than 0.");

            var defaultStatus = Enum.TryParse(_options.DefaultShipmentStatus, true, out ShipmentStatus parsedStatus)
                ? parsedStatus
                : ShipmentStatus.Planned;

            var entity = new Shipment
            {
                Reference = dto.Reference.Trim(),
                Status = defaultStatus,
                DistanceKm = dto.DistanceKm,
                WeightKg = dto.WeightKg,
                CustomerId = dto.CustomerId,
                VehicleId = dto.VehicleId,
                CreatedUtc = DateTime.UtcNow
            };

            await _shipments.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            var created = await _shipments.GetByIdAsync(entity.Id, ct);
            if (created == null) 
                created = entity;

            var result = new ShipmentResponseDto
            {
                Id = created.Id,
                Reference = created.Reference,
                Status = created.Status,
                DistanceKm = created.DistanceKm,
                WeightKg = created.WeightKg,
                CreatedUtc = created.CreatedUtc,
                CustomerId = created.CustomerId,
                CustomerName = created.Customer?.Name,
                VehicleId = created.VehicleId,
                VehiclePlate = created.Vehicle?.PlateNumber,
                EstimatedPrice = Math.Round(
                    _options.BasePricePerKm * created.DistanceKm +
                    _options.WeightPricePerKg * created.WeightKg, 2),
                Currency = _options.Currency ?? "USD"
            };

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // PUT: api/v1/shipments/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ShipmentCreateUpdateDto dto, CancellationToken ct)
        {
            if (id <= 0) return BadRequest("Id must be greater than 0.");
            if (dto == null || string.IsNullOrWhiteSpace(dto.Reference))
                return BadRequest("Reference is required.");
            if (dto.DistanceKm <= 0)
                return BadRequest("DistanceKm must be greater than 0.");

            var entity = await _shipments.GetByIdAsync(id, ct);
            if (entity == null) return NotFound($"Shipment with id {id} not found.");

            entity.Reference = dto.Reference.Trim();
            entity.DistanceKm = dto.DistanceKm;
            entity.WeightKg = dto.WeightKg;
            entity.CustomerId = dto.CustomerId;
            entity.VehicleId = dto.VehicleId;

            _shipments.Update(entity);
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }

        // DELETE: api/v1/shipments/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            if (id <= 0) return BadRequest("Id must be greater than 0.");

            var entity = await _shipments.GetByIdAsync(id, ct);
            if (entity == null) return NotFound($"Shipment with id {id} not found.");

            _shipments.Remove(entity);
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
