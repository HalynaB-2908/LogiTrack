using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using LogiTrack.WebApi.Contracts.Shipments;
using LogiTrack.WebApi.Models;
using LogiTrack.WebApi.Options;
using LogiTrack.WebApi.Services.Abstractions;
using LogiTrack.WebApi.Services.Factories;
using IO = System.IO;

namespace LogiTrack.WebApi.Controllers
{
    /// <summary>
    /// Controller for managing shipments in the logistics system.
    /// Provides endpoints for CRUD operations, search and export of shipment data.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin,User")]
    public class ShipmentsController : ControllerBase
    {
        private readonly LogisticsOptions _options;
        private readonly IUnitOfWork _uow;
        private readonly DeliveryTimeServiceFactory _factory;
        private readonly ILogger<ShipmentsController> _logger;

        /// <summary>
        /// Initializes a new instance of the ShipmentsController.
        /// </summary>
        public ShipmentsController(
            IOptions<LogisticsOptions> options,
            IUnitOfWork uow,
            DeliveryTimeServiceFactory factory,
            ILogger<ShipmentsController> logger)
        {
            _options = options.Value;
            _uow = uow;
            _factory = factory;
            _logger = logger;
        }

        /// <summary>
        /// Returns all shipments.
        /// </summary>
        /// <response code="200">List of shipments successfully returned</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            _logger.LogInformation("Getting all shipments");

            var entities = await _uow.Shipments.GetAllAsync(ct);

            var items = entities
                .OrderBy(s => s.Id)
                .Select(s =>
                {
                    double price = s.Price.HasValue
                        ? (double)s.Price.Value
                        : _options.BasePricePerKm * s.DistanceKm +
                          _options.WeightPricePerKg * s.WeightKg;

                    var service = _factory.CreateService("standard");
                    double hours = s.DistanceKm > 0
                        ? service.Estimate(s.DistanceKm)
                        : 0;

                    return new ShipmentResponseDto
                    {
                        Id = s.Id,
                        Reference = s.Reference,
                        Status = s.Status,
                        DistanceKm = s.DistanceKm,
                        WeightKg = s.WeightKg,
                        CreatedUtc = s.CreatedUtc,
                        CustomerId = s.CustomerId,
                        CustomerName = s.Customer != null ? s.Customer.Name : null,
                        VehicleId = s.VehicleId,
                        VehiclePlate = s.Vehicle != null ? s.Vehicle.PlateNumber : null,
                        EstimatedPrice = Math.Round(price, 2),
                        EstimatedTimeHours = Math.Round(hours, 2),
                        Currency = _options.Currency ?? "EUR"
                    };
                })
                .ToList();

            _logger.LogInformation("Returning {Count} shipments", items.Count);

            return Ok(items);
        }

        /// <summary>
        /// Returns a shipment by its identifier.
        /// </summary>
        /// <param name="id">Shipment identifier</param>
        /// <param name="ct">Cancellation token for the asynchronous operation.</param>
        /// <response code="200">Shipment found and returned</response>
        /// <response code="400">Invalid id</response>
        /// <response code="404">Shipment not found</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
        {
            if (id <= 0)
            {
                _logger.LogWarning("GetById called with invalid id {Id}", id);
                return BadRequest("Id must be greater than 0.");
            }

            var s = await _uow.Shipments.GetByIdAsync(id, ct);
            if (s == null)
            {
                _logger.LogWarning("Shipment with id {Id} not found", id);
                return NotFound($"Shipment with id {id} not found.");
            }

            double price = s.Price.HasValue
                ? (double)s.Price.Value
                : _options.BasePricePerKm * s.DistanceKm +
                  _options.WeightPricePerKg * s.WeightKg;

            var service = _factory.CreateService("standard");
            double hours = s.DistanceKm > 0
                ? service.Estimate(s.DistanceKm)
                : 0;

            var dto = new ShipmentResponseDto
            {
                Id = s.Id,
                Reference = s.Reference,
                Status = s.Status,
                DistanceKm = s.DistanceKm,
                WeightKg = s.WeightKg,
                CreatedUtc = s.CreatedUtc,
                CustomerId = s.CustomerId,
                CustomerName = s.Customer != null ? s.Customer.Name : null,
                VehicleId = s.VehicleId,
                VehiclePlate = s.Vehicle != null ? s.Vehicle.PlateNumber : null,
                EstimatedPrice = Math.Round(price, 2),
                EstimatedTimeHours = Math.Round(hours, 2),
                Currency = _options.Currency ?? "EUR"
            };

            return Ok(dto);
        }

        /// <summary>
        /// Creates a new shipment.
        /// </summary>
        /// <param name="dto">Shipment data</param>
        /// <param name="ct">Cancellation token for the asynchronous operation.</param>
        /// <response code="201">Shipment successfully created</response>
        /// <response code="400">Invalid input data</response>
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

            var entity = new Shipment
            {
                Reference = dto.Reference.Trim(),
                Status = ShipmentStatus.Planned,
                DistanceKm = dto.DistanceKm,
                WeightKg = dto.WeightKg,
                CustomerId = dto.CustomerId,
                VehicleId = dto.VehicleId,
                CreatedUtc = DateTime.UtcNow
            };

            double price = _options.BasePricePerKm * entity.DistanceKm +
                           _options.WeightPricePerKg * entity.WeightKg;

            entity.Price = (decimal)Math.Round(price, 2);

            await _uow.Shipments.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
        }
    }
}
