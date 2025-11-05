using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogiTrack.WebApi.Data;
using LogiTrack.WebApi.Models;
using LogiTrack.WebApi.Contracts.Drivers;

namespace LogiTrack.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin,User")]
    public class DriversController : ControllerBase
    {
        private readonly LogiTrackDbContext _db;

        public DriversController(LogiTrackDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
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

            return Ok(items);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            if (id <= 0) return BadRequest("Id must be greater than 0.");

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

            if (item == null) return NotFound($"Driver with id {id} not found.");
            return Ok(item);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] DriverCreateUpdateDto dto)
        {
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

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] DriverCreateUpdateDto dto)
        {
            if (id <= 0) return BadRequest("Id must be greater than 0.");

            var entity = await _db.Drivers.FirstOrDefaultAsync(d => d.Id == id);
            if (entity == null) return NotFound($"Driver with id {id} not found.");

            entity.FullName = dto.FullName.Trim();
            entity.Phone = dto.Phone;

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

            var entity = await _db.Drivers.FirstOrDefaultAsync(d => d.Id == id);
            if (entity == null) return NotFound($"Driver with id {id} not found.");

            _db.Drivers.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
