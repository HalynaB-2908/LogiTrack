using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        // Get a customer by id (route parameter)
        [HttpGet("{id:int}")]
        public IActionResult GetById([FromRoute] int id)
        {
            if (id <= 0) return BadRequest("Id must be greater than 0.");

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

