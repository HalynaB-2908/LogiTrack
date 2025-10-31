using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogiTrack.WebApi.Data;
using LogiTrack.WebApi.Models;
using System.ComponentModel.DataAnnotations;

namespace LogiTrack.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin,User")]
    public class VehiclesController : ControllerBase
    {
        private readonly LogiTrackDbContext _db;

        public VehiclesController(LogiTrackDbContext db)
        {
            _db = db;
        }

        public class VehicleDto
        {
            public int Id { get; set; }
            public string PlateNumber { get; set; } = default!;
            public string Model { get; set; } = default!;
            public double CapacityKg { get; set; }
            public int? DriverId { get; set; }
            public string? DriverName { get; set; }
        }

        public class UpsertVehicleDto
        {
            [Required, MaxLength(50)]
            public string PlateNumber { get; set; } = default!;

            [Required, MaxLength(200)]
            public string Model { get; set; } = default!;

            public double CapacityKg { get; set; }

            public int? DriverId { get; set; }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.Vehicles
                .AsNoTracking()
                .Include(v => v.Driver)
                .OrderBy(v => v.Id)
                .Select(v => new VehicleDto
                {
                    Id = v.Id,
                    PlateNumber = v.PlateNumber,
                    Model = v.Model,
                    CapacityKg = v.CapacityKg,
                    DriverId = v.DriverId,
                    DriverName = v.Driver != null ? v.Driver.FullName : null
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

            var item = await _db.Vehicles
                .AsNoTracking()
                .Include(v => v.Driver)
                .Where(v => v.Id == id)
                .Select(v => new VehicleDto
                {
                    Id = v.Id,
                    PlateNumber = v.PlateNumber,
                    Model = v.Model,
                    CapacityKg = v.CapacityKg,
                    DriverId = v.DriverId,
                    DriverName = v.Driver != null ? v.Driver.FullName : null
                })
                .FirstOrDefaultAsync();

            if (item == null) return NotFound($"Vehicle with id {id} not found.");

            return Ok(item);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] UpsertVehicleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entity = new Vehicle
            {
                PlateNumber = dto.PlateNumber.Trim(),
                Model = dto.Model.Trim(),
                CapacityKg = dto.CapacityKg,
                DriverId = dto.DriverId
            };

            _db.Vehicles.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new VehicleDto
            {
                Id = entity.Id,
                PlateNumber = entity.PlateNumber,
                Model = entity.Model,
                CapacityKg = entity.CapacityKg,
                DriverId = entity.DriverId,
                DriverName = null
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpsertVehicleDto dto)
        {
            if (id <= 0) return BadRequest("Id must be greater than 0.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entity = await _db.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
            if (entity == null) return NotFound($"Vehicle with id {id} not found.");

            entity.PlateNumber = dto.PlateNumber.Trim();
            entity.Model = dto.Model.Trim();
            entity.CapacityKg = dto.CapacityKg;
            entity.DriverId = dto.DriverId;

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

            var entity = await _db.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
            if (entity == null) return NotFound($"Vehicle with id {id} not found.");

            _db.Vehicles.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
