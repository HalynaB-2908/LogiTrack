using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class CustomersController : ControllerBase
    {
        // GET /api/v1/customers/{id}
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById([FromRoute] int id)
        {
            if (id <= 0) return BadRequest("Id must be greater than 0.");

            // demo
            var demoCustomer = new
            {
                Id = id,
                Name = "Demo Customer",
                Email = "demo@example.com"
            };

            return Ok(demoCustomer);
        }
    }
}

