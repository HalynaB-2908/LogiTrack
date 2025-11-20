using LogiTrack.WebApi.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin")]
    public class MetricsController : ControllerBase
    {
        private readonly IApiMetricsService _metrics;

        public MetricsController(IApiMetricsService metrics)
        {
            _metrics = metrics;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            var perControllerCounts = _metrics.GetPerControllerCounts();

            var result = new
            {
                TotalRequests = _metrics.TotalRequests,
                AverageResponseTimeMs = Math.Round(_metrics.AverageResponseTimeMs, 2),
                PerController = perControllerCounts
                    .OrderBy(kvp => kvp.Key)
                    .Select(kvp => new
                    {
                        Controller = kvp.Key,
                        Count = kvp.Value
                    })
                    .ToList()
            };

            return Ok(result);
        }
    }
}
