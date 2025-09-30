using Microsoft.AspNetCore.Mvc;
using LogiTrack.WebApi.Contracts;

namespace LogiTrack.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentsController : ControllerBase
    {
        // Get a shipment by id (route parameter)
        [HttpGet("{id:int}")]
        public IActionResult GetById([FromRoute] int id)
        {
            if (id <= 0) return BadRequest("Id must be greater than 0.");

            var demoShipment = new
            {
                Id = id,
                Reference = "DEMO-SHP-001",
                Status = "Planned"
            };

            return Ok(demoShipment);
        }

        // Search shipments (query parameters)
        [HttpGet("search")]
        public IActionResult Search([FromQuery] string? q, [FromQuery] string? status)
        {
            if (string.IsNullOrWhiteSpace(q) && string.IsNullOrWhiteSpace(status))
                return BadRequest("At least one search parameter must be provided.");

            var results = new[]
            {
                new { Id = 1, Reference = "DEMO-SHP-001", Status = status ?? "Planned" }
            };

            return Ok(results);
        }

        // Create a new shipment (body)
        [HttpPost]
        public IActionResult Create([FromBody] CreateShipmentDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Reference))
                return BadRequest("Reference is required.");

            var newShipment = new
            {
                Id = 99,
                Reference = request.Reference,
                Status = "Created"
            };

            return CreatedAtAction(nameof(GetById), new { id = newShipment.Id }, newShipment);
        }
    }
}
