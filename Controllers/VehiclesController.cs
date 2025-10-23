using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogiTrack.WebApi.Data;

namespace LogiTrack.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class VehiclesController : ControllerBase
    {
        private readonly LogiTrackDbContext _db;

        public VehiclesController(LogiTrackDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.Vehicles
                .AsNoTracking()
                .Include(v => v.Driver)
                .Select(v => new
                {
                    v.Id,
                    v.PlateNumber,
                    v.Model,
                    v.CapacityKg,
                    v.DriverId,
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
                .Select(v => new
                {
                    v.Id,
                    v.PlateNumber,
                    v.Model,
                    v.CapacityKg,
                    v.DriverId,
                    DriverName = v.Driver != null ? v.Driver.FullName : null
                })
                .FirstOrDefaultAsync();

            if (item == null) return NotFound($"Vehicle with id {id} not found.");

            return Ok(item);
        }
    }
}


