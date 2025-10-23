using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogiTrack.WebApi.Data;

namespace LogiTrack.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class CustomersController : ControllerBase
    {
        private readonly LogiTrackDbContext _db;

        public CustomersController(LogiTrackDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.Customers
                .AsNoTracking()
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Email,
                    c.Phone,
                    c.Address
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

            var item = await _db.Customers
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Email,
                    c.Phone,
                    c.Address
                })
                .FirstOrDefaultAsync();

            if (item == null) return NotFound($"Customer with id {id} not found.");

            return Ok(item);
        }
    }
}

