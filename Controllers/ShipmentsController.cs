using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using LogiTrack.WebApi.Contracts;
using LogiTrack.WebApi.Options;
using LogiTrack.WebApi.Services;

namespace LogiTrack.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class ShipmentsController : ControllerBase
    {
        private readonly LogisticsOptions _options;
        private readonly IDeliveryTimeService _delivery; // service via DI

        // Inject configuration and custom service
        public ShipmentsController(IOptions<LogisticsOptions> options, IDeliveryTimeService delivery)
        {
            _options = options.Value;
            _delivery = delivery;
        }

        // GET /api/v1/shipments/{id}
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById([FromRoute] int id)
        {
            if (id <= 0) return BadRequest("Id must be greater than 0.");

            var demoShipment = new
            {
                Id = id,
                Reference = "DEMO-SHP-001",
                // from appsettings.json (Options)
                Status = _options.DefaultShipmentStatus
            };

            return Ok(demoShipment);
        }

        // GET /api/v1/shipments/search?q=&status=
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Search([FromQuery] string? q, [FromQuery] string? status)
        {
            if (string.IsNullOrWhiteSpace(q) && string.IsNullOrWhiteSpace(status))
                return BadRequest("At least one search parameter must be provided.");

            var results = new[]
            {
                new { Id = 1, Reference = "DEMO-SHP-001", Status = status ?? _options.DefaultShipmentStatus }
            };

            return Ok(results);
        }

        // POST /api/v1/shipments
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Create([FromBody] CreateShipmentDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Reference))
                return BadRequest("Reference is required.");
            if (request.DistanceKm <= 0)
                return BadRequest("DistanceKm must be greater than 0.");

            // compute ETA using service
            var etaHours = _delivery.Estimate(request.DistanceKm);

            // simple demo price using Options
            var estimatedPrice = _options.BasePricePerKm * request.DistanceKm
                               + _options.WeightPricePerKg * (request.WeightKg);

            var newShipment = new
            {
                Id = 99,
                Reference = request.Reference,
                Status = _options.DefaultShipmentStatus,
                EstimatedTimeHours = Math.Round(etaHours, 2),
                EstimatedArrival= DateTime.Now.AddHours(etaHours).ToString("yyyy-MM-dd HH:mm"),
                EstimatedPrice = Math.Round(estimatedPrice, 2),
                Currency = _options.Currency
            };

            return CreatedAtAction(nameof(GetById), new { id = newShipment.Id }, newShipment);
        }
    }
}
