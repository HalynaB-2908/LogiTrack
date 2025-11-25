using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogiTrack.WebApi.Data;
using LogiTrack.WebApi.Models;
using LogiTrack.WebApi.Contracts.Vehicles;

namespace LogiTrack.WebApi.Controllers
{
    /// <summary>
    /// Controller responsible for managing vehicles in the system.
    /// Provides endpoints for CRUD operations on vehicles and their assigned drivers.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin,User")]
    public class VehiclesController : ControllerBase
    {
        private readonly LogiTrackDbContext _db;
        private readonly ILogger<VehiclesController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="VehiclesController"/> class.
        /// </summary>
        /// <param name="db">Database context for accessing vehicle data.</param>
        /// <param name="logger">Logger for controller diagnostics.</param>
        public VehiclesController(LogiTrackDbContext db, ILogger<VehiclesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Returns a list of all vehicles.
        /// </summary>
        /// <returns>List of vehicles with their basic information and assigned driver.</returns>
        /// <response code="200">List of vehicles successfully returned.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Getting all vehicles");

            var items = await _db.Vehicles
                .AsNoTracking()
                .Include(v => v.Driver)
                .OrderBy(v => v.Id)
                .Select(v => new VehicleResponseDto
                {
                    Id = v.Id,
                    PlateNumber = v.PlateNumber,
                    Model = v.Model,
                    CapacityKg = v.CapacityKg,
                    DriverId = v.DriverId,
                    DriverName = v.Driver != null ? v.Driver.FullName : null
                })
                .ToListAsync();

            _logger.LogInformation("Returning {Count} vehicles", items.Count);

            return Ok(items);
        }

        /// <summary>
        /// Returns a specific vehicle by its identifier.
        /// </summary>
        /// <param name="id">Vehicle identifier.</param>
        /// <returns>Vehicle data.</returns>
        /// <response code="200">Vehicle successfully returned.</response>
        /// <response code="400">Invalid vehicle id.</response>
        /// <response code="404">Vehicle not found.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("GetById called with invalid id {Id}", id);
                return BadRequest("Id must be greater than 0.");
            }

            _logger.LogInformation("Getting vehicle by id {Id}", id);

            var item = await _db.Vehicles
                .AsNoTracking()
                .Include(v => v.Driver)
                .Where(v => v.Id == id)
                .Select(v => new VehicleResponseDto
                {
                    Id = v.Id,
                    PlateNumber = v.PlateNumber,
                    Model = v.Model,
                    CapacityKg = v.CapacityKg,
                    DriverId = v.DriverId,
                    DriverName = v.Driver != null ? v.Driver.FullName : null
                })
                .FirstOrDefaultAsync();

            if (item == null)
            {
                _logger.LogWarning("Vehicle with id {Id} not found", id);
                return NotFound($"Vehicle with id {id} not found.");
            }

            return Ok(item);
        }

        /// <summary>
        /// Creates a new vehicle.
        /// </summary>
        /// <param name="dto">Vehicle creation data.</param>
        /// <returns>Created vehicle.</returns>
        /// <response code="201">Vehicle successfully created.</response>
        /// <response code="400">Invalid input data.</response>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] VehicleCreateUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Create vehicle failed: invalid model state");
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating new vehicle with plate {Plate}", dto.PlateNumber);

            var entity = new Vehicle
            {
                PlateNumber = dto.PlateNumber.Trim(),
                Model = dto.Model.Trim(),
                CapacityKg = dto.CapacityKg,
                DriverId = dto.DriverId
            };

            _db.Vehicles.Add(entity);
            await _db.SaveChangesAsync();

            string? driverName = null;
            if (entity.DriverId.HasValue)
            {
                driverName = await _db.Drivers
                    .Where(d => d.Id == entity.DriverId.Value)
                    .Select(d => d.FullName)
                    .FirstOrDefaultAsync();
            }

            var result = new VehicleResponseDto
            {
                Id = entity.Id,
                PlateNumber = entity.PlateNumber,
                Model = entity.Model,
                CapacityKg = entity.CapacityKg,
                DriverId = entity.DriverId,
                DriverName = driverName
            };

            _logger.LogInformation("Vehicle created successfully with id {Id}", entity.Id);

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
        }

        /// <summary>
        /// Updates an existing vehicle.
        /// </summary>
        /// <param name="id">Vehicle identifier.</param>
        /// <param name="dto">Updated vehicle data.</param>
        /// <returns>No content if update successful.</returns>
        /// <response code="204">Vehicle successfully updated.</response>
        /// <response code="400">Invalid input data.</response>
        /// <response code="404">Vehicle not found.</response>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] VehicleCreateUpdateDto dto)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Update vehicle called with invalid id {Id}", id);
                return BadRequest("Id must be greater than 0.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Update vehicle {Id} failed: invalid model state", id);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Updating vehicle with id {Id}", id);

            var entity = await _db.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
            if (entity == null)
            {
                _logger.LogWarning("Vehicle with id {Id} not found for update", id);
                return NotFound($"Vehicle with id {id} not found.");
            }

            entity.PlateNumber = dto.PlateNumber.Trim();
            entity.Model = dto.Model.Trim();
            entity.CapacityKg = dto.CapacityKg;
            entity.DriverId = dto.DriverId;

            await _db.SaveChangesAsync();

            _logger.LogInformation("Vehicle with id {Id} updated successfully", id);

            return NoContent();
        }

        /// <summary>
        /// Deletes a vehicle by its identifier.
        /// </summary>
        /// <param name="id">Vehicle identifier.</param>
        /// <returns>No content if deletion is successful.</returns>
        /// <response code="204">Vehicle successfully deleted.</response>
        /// <response code="400">Invalid vehicle id.</response>
        /// <response code="404">Vehicle not found.</response>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Delete vehicle called with invalid id {Id}", id);
                return BadRequest("Id must be greater than 0.");
            }

            _logger.LogInformation("Deleting vehicle with id {Id}", id);

            var entity = await _db.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
            if (entity == null)
            {
                _logger.LogWarning("Vehicle with id {Id} not found for delete", id);
                return NotFound($"Vehicle with id {id} not found.");
            }

            _db.Vehicles.Remove(entity);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Vehicle with id {Id} deleted successfully", id);

            return NoContent();
        }
    }
}
