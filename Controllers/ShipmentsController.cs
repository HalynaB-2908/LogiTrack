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
    /// Controller responsible for managing shipments.
    /// Provides endpoints for retrieving, searching, creating, updating, deleting and exporting shipment data.
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
        /// Initializes a new instance of the <see cref="ShipmentsController"/> class.
        /// </summary>
        /// <param name="options">Logistics configuration options used for price and currency calculation.</param>
        /// <param name="uow">Unit of Work used to access repositories and save changes.</param>
        /// <param name="factory">Factory used to create delivery time calculation services.</param>
        /// <param name="logger">Logger instance for shipment controller diagnostics.</param>
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
        /// <param name="ct">Cancellation token for the asynchronous operation.</param>
        /// <returns>List of shipments with calculated price and estimated delivery time.</returns>
        /// <response code="200">Shipments successfully returned.</response>
        // GET ALL
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
        /// <param name="id">Identifier of the shipment.</param>
        /// <param name="ct">Cancellation token for the asynchronous operation.</param>
        /// <returns>Shipment information if found.</returns>
        /// <response code="200">Shipment successfully returned.</response>
        /// <response code="400">Invalid shipment identifier.</response>
        /// <response code="404">Shipment not found.</response>
        // GET BY ID
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

            _logger.LogInformation("Getting shipment by id {Id}", id);

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
        /// Searches shipments by text query and optional shipment status.
        /// </summary>
        /// <param name="q">Optional search text used to filter shipments by reference or related data.</param>
        /// <param name="status">Optional shipment status filter.</param>
        /// <param name="ct">Cancellation token for the asynchronous operation.</param>
        /// <returns>Filtered list of shipments.</returns>
        /// <response code="200">Search completed successfully.</response>
        /// <response code="400">Invalid shipment status value.</response>
        // SEARCH
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Search(
            [FromQuery] string? q,
            [FromQuery] string? status,
            CancellationToken ct)
        {
            _logger.LogInformation("Searching shipments. q={Query}, status={Status}", q, status);

            ShipmentStatus? parsedStatus = null;

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (!Enum.TryParse(status, true, out ShipmentStatus value))
                {
                    _logger.LogWarning("Unknown shipment status in search: {Status}", status);
                    return BadRequest(
                        $"Unknown status '{status}'. Allowed: {string.Join(", ", Enum.GetNames(typeof(ShipmentStatus)))}"
                    );
                }

                parsedStatus = value;
            }

            var entities = await _uow.Shipments.SearchAsync(q, parsedStatus, ct);

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

            _logger.LogInformation("Search returned {Count} shipments", items.Count);

            return Ok(items);
        }

        /// <summary>
        /// Creates a new shipment.
        /// </summary>
        /// <param name="dto">Shipment creation data.</param>
        /// <param name="ct">Cancellation token for the asynchronous operation.</param>
        /// <returns>Created shipment information.</returns>
        /// <response code="201">Shipment successfully created.</response>
        /// <response code="400">Invalid shipment data.</response>
        // CREATE
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
            [FromBody] ShipmentCreateUpdateDto dto,
            CancellationToken ct)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Reference))
            {
                _logger.LogWarning("Create shipment failed: Reference is missing");
                return BadRequest("Reference is required.");
            }

            if (dto.DistanceKm <= 0)
            {
                _logger.LogWarning("Create shipment failed: invalid DistanceKm {DistanceKm}", dto.DistanceKm);
                return BadRequest("DistanceKm must be greater than 0.");
            }

            _logger.LogInformation("Creating new shipment with reference {Reference}", dto.Reference);

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

            var service = _factory.CreateService(dto.DeliveryMode);
            double hours = service.Estimate(entity.DistanceKm);

            double price =
                _options.BasePricePerKm * entity.DistanceKm +
                _options.WeightPricePerKg * entity.WeightKg;

            entity.Price = (decimal)Math.Round(price, 2);

            await _uow.Shipments.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            var created = await _uow.Shipments.GetByIdAsync(entity.Id, ct);
            if (created == null)
            {
                _logger.LogError("Shipment was created but cannot be loaded. Id={Id}", entity.Id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Shipment was created but cannot be loaded.");
            }

            var serviceForResult = _factory.CreateService(dto.DeliveryMode);
            double hoursResult = serviceForResult.Estimate(created.DistanceKm);

            double resultPrice = created.Price.HasValue
                ? (double)created.Price.Value
                : _options.BasePricePerKm * created.DistanceKm +
                  _options.WeightPricePerKg * created.WeightKg;

            var result = new ShipmentResponseDto
            {
                Id = created.Id,
                Reference = created.Reference,
                Status = created.Status,
                DistanceKm = created.DistanceKm,
                WeightKg = created.WeightKg,
                CreatedUtc = created.CreatedUtc,
                CustomerId = created.CustomerId,
                CustomerName = created.Customer != null ? created.Customer.Name : null,
                VehicleId = created.VehicleId,
                VehiclePlate = created.Vehicle != null ? created.Vehicle.PlateNumber : null,
                EstimatedPrice = Math.Round(resultPrice, 2),
                EstimatedTimeHours = Math.Round(hoursResult, 2),
                Currency = _options.Currency ?? "EUR"
            };

            _logger.LogInformation("Shipment created successfully with id {Id}", result.Id);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Updates an existing shipment.
        /// </summary>
        /// <param name="id">Identifier of the shipment to update.</param>
        /// <param name="dto">Updated shipment data.</param>
        /// <param name="ct">Cancellation token for the asynchronous operation.</param>
        /// <returns>No content if the shipment was updated successfully.</returns>
        /// <response code="204">Shipment successfully updated.</response>
        /// <response code="400">Invalid shipment id or invalid shipment data.</response>
        /// <response code="404">Shipment not found.</response>
        // UPDATE
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            [FromRoute] int id,
            [FromBody] ShipmentCreateUpdateDto dto,
            CancellationToken ct)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Update shipment called with invalid id {Id}", id);
                return BadRequest("Id must be greater than 0.");
            }

            if (dto == null || string.IsNullOrWhiteSpace(dto.Reference))
            {
                _logger.LogWarning("Update shipment {Id} failed: Reference is missing", id);
                return BadRequest("Reference is required.");
            }

            if (dto.DistanceKm <= 0)
            {
                _logger.LogWarning("Update shipment {Id} failed: invalid DistanceKm {DistanceKm}", id, dto.DistanceKm);
                return BadRequest("DistanceKm must be greater than 0.");
            }

            _logger.LogInformation("Updating shipment with id {Id}", id);

            var entity = await _uow.Shipments.GetByIdAsync(id, ct);
            if (entity == null)
            {
                _logger.LogWarning("Shipment with id {Id} not found for update", id);
                return NotFound($"Shipment with id {id} not found.");
            }

            entity.Reference = dto.Reference.Trim();
            entity.DistanceKm = dto.DistanceKm;
            entity.WeightKg = dto.WeightKg;
            entity.CustomerId = dto.CustomerId;
            entity.VehicleId = dto.VehicleId;

            double price =
                _options.BasePricePerKm * entity.DistanceKm +
                _options.WeightPricePerKg * entity.WeightKg;

            entity.Price = (decimal)Math.Round(price, 2);

            _uow.Shipments.Update(entity);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Shipment with id {Id} updated successfully", id);

            return NoContent();
        }

        /// <summary>
        /// Deletes a shipment by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the shipment to delete.</param>
        /// <param name="ct">Cancellation token for the asynchronous operation.</param>
        /// <returns>No content if the shipment was deleted successfully.</returns>
        /// <response code="204">Shipment successfully deleted.</response>
        /// <response code="400">Invalid shipment identifier.</response>
        /// <response code="404">Shipment not found.</response>
        // DELETE
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Delete shipment called with invalid id {Id}", id);
                return BadRequest("Id must be greater than 0.");
            }

            _logger.LogInformation("Deleting shipment with id {Id}", id);

            var entity = await _uow.Shipments.GetByIdAsync(id, ct);
            if (entity == null)
            {
                _logger.LogWarning("Shipment with id {Id} not found for delete", id);
                return NotFound($"Shipment with id {id} not found.");
            }

            _uow.Shipments.Remove(entity);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Shipment with id {Id} deleted successfully", id);

            return NoContent();
        }

        /// <summary>
        /// Exports shipment data as a JSON file.
        /// </summary>
        /// <param name="storage">Storage options containing the shipments file path.</param>
        /// <param name="env">Web host environment used to resolve relative file paths.</param>
        /// <returns>JSON file with shipment data.</returns>
        /// <response code="200">Shipment export file successfully returned.</response>
        /// <response code="404">Shipment export file not found.</response>
        // EXPORT
        [HttpGet("export")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Export(
            [FromServices] IOptions<StorageOptions> storage,
            [FromServices] IWebHostEnvironment env)
        {
            _logger.LogInformation("Exporting shipments to JSON file");

            var path = storage.Value.ShipmentsFilePath;
            if (!IO.Path.IsPathRooted(path))
                path = IO.Path.Combine(env.ContentRootPath, path);

            if (!IO.File.Exists(path))
            {
                _logger.LogWarning("Shipments export file not found at path {Path}", path);
                return NotFound("Data file not found.");
            }

            var bytes = IO.File.ReadAllBytes(path);

            _logger.LogInformation("Shipments export file loaded successfully from {Path}", path);

            return File(bytes, "application/json", "shipments.json");
        }
    }
}
