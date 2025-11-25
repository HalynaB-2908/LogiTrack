using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogiTrack.WebApi.Data;
using LogiTrack.WebApi.Models;
using LogiTrack.WebApi.Contracts.Drivers;

namespace LogiTrack.WebApi.Controllers
{
    /// <summary>
    /// Controller responsible for managing drivers.
    /// Provides CRUD operations for driver entities.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin,User")]
    public class DriversController : ControllerBase
    {
        private readonly LogiTrackDbContext _db;
        private readonly ILogger<DriversController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DriversController"/> class.
        /// </summary>
        /// <param name="db">Database context.</param>
        /// <param name="logger">Logger instance.</param>
        public DriversController(LogiTrackDbContext db, ILogger<DriversController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Returns a list of all drivers.
        /// </summary>
        /// <returns>List of drivers.</returns>
        /// <response code="200">Drivers successfully retrieved.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Getting all drivers");

            var items = await _db.Drivers
                .AsNoTracking()
                .OrderBy(d => d.Id)
                .Select(d => new DriverResponseDto
                {
                    Id = d.Id,
                    FullName = d.FullName,
                    Phone = d.Phone
                })
                .ToListAsync();

            _logger.LogInformation("Returning {Count} drivers", items.Count);

            return Ok(items);
        }

        /// <summary>
        /// Returns driver by id.
        /// </summary>
        /// <param name="id">Driver identifier.</param>
        /// <returns>Driver data.</returns>
        /// <response code="200">Driver found.</response>
        /// <response code="400">Invalid id supplied.</response>
        /// <response code="404">Driver not found.</response>
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

            _logger.LogInformation("Getting driver by id {Id}", id);

            var item = await _db.Drivers
                .AsNoTracking()
                .Where(d => d.Id == id)
                .Select(d => new DriverResponseDto
                {
                    Id = d.Id,
                    FullName = d.FullName,
                    Phone = d.Phone
                })
                .FirstOrDefaultAsync();

            if (item == null)
            {
                _logger.LogWarning("Driver with id {Id} not found", id);
                return NotFound($"Driver with id {id} not found.");
            }

            return Ok(item);
        }

        /// <summary>
        /// Creates a new driver.
        /// </summary>
        /// <param name="dto">Driver creation data.</param>
        /// <returns>Created driver.</returns>
        /// <response code="201">Driver successfully created.</response>
        /// <response code="400">Invalid request data.</response>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] DriverCreateUpdateDto dto)
        {
            _logger.LogInformation("Creating new driver {Name}", dto.FullName);

            var entity = new Driver
            {
                FullName = dto.FullName.Trim(),
                Phone = dto.Phone
            };

            _db.Drivers.Add(entity);
            await _db.SaveChangesAsync();

            var result = new DriverResponseDto
            {
                Id = entity.Id,
                FullName = entity.FullName,
                Phone = entity.Phone
            };

            _logger.LogInformation("Driver created successfully with id {Id}", entity.Id);

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
        }

        /// <summary>
        /// Updates an existing driver.
        /// </summary>
        /// <param name="id">Driver identifier.</param>
        /// <param name="dto">Updated driver data.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">Driver successfully updated.</response>
        /// <response code="400">Invalid id.</response>
        /// <response code="404">Driver not found.</response>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] DriverCreateUpdateDto dto)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Update called with invalid id {Id}", id);
                return BadRequest("Id must be greater than 0.");
            }

            _logger.LogInformation("Updating driver with id {Id}", id);

            var entity = await _db.Drivers.FirstOrDefaultAsync(d => d.Id == id);
            if (entity == null)
            {
                _logger.LogWarning("Driver with id {Id} not found for update", id);
                return NotFound($"Driver with id {id} not found.");
            }

            entity.FullName = dto.FullName.Trim();
            entity.Phone = dto.Phone;

            await _db.SaveChangesAsync();

            _logger.LogInformation("Driver with id {Id} updated successfully", id);

            return NoContent();
        }

        /// <summary>
        /// Deletes a driver by id.
        /// </summary>
        /// <param name="id">Driver identifier.</param>
        /// <returns>No content if deleted.</returns>
        /// <response code="204">Driver successfully deleted.</response>
        /// <response code="400">Invalid id.</response>
        /// <response code="404">Driver not found.</response>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Delete called with invalid id {Id}", id);
                return BadRequest("Id must be greater than 0.");
            }

            _logger.LogInformation("Deleting driver with id {Id}", id);

            var entity = await _db.Drivers.FirstOrDefaultAsync(d => d.Id == id);
            if (entity == null)
            {
                _logger.LogWarning("Driver with id {Id} not found for delete", id);
                return NotFound($"Driver with id {id} not found.");
            }

            _db.Drivers.Remove(entity);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Driver with id {Id} deleted successfully", id);

            return NoContent();
        }
    }
}
