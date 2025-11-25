using LogiTrack.WebApi.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.WebApi.Controllers
{
    /// <summary>
    /// Controller for accessing API usage statistics and performance metrics.
    /// Provides aggregated information about total requests and per-controller usage.
    /// Access is allowed only for users with the Admin role.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin")]
    public class MetricsController : ControllerBase
    {
        private readonly IApiMetricsService _metrics;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricsController"/> class.
        /// </summary>
        /// <param name="metrics">Service that collects API metrics and statistics.</param>
        public MetricsController(IApiMetricsService metrics)
        {
            _metrics = metrics;
        }

        /// <summary>
        /// Returns aggregated API metrics including total request count,
        /// average response time and per-controller request statistics.
        /// </summary>
        /// <returns>
        /// Object containing API metrics summary.
        /// </returns>
        /// <response code="200">Metrics successfully returned.</response>
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
