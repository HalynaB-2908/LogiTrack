using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : ControllerBase
    {
        // Get a vehicle by id (route parameter)
        [HttpGet("{id:int}")]
        public IActionResult GetById([FromRoute] int id)
        {
            if (id <= 0) return BadRequest("Id must be greater than 0.");

            var demoVehicle = new
            {
                Id = id,
                PlateNumber = "DEMO-123",
                Model = "Mercedes Actros"
            };

            return Ok(demoVehicle);
        }
    }
}

