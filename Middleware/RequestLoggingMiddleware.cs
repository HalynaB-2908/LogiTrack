using System.Diagnostics;
using System.Threading.Tasks;
using LogiTrack.WebApi.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;

namespace LogiTrack.WebApi.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly IApiMetricsService _metrics;

        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger,
            IApiMetricsService metrics)
        {
            _next = next;
            _logger = logger;
            _metrics = metrics;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            var method = context.Request.Method;
            var path = context.Request.Path;

            var userName = context.User?.Identity?.IsAuthenticated == true
                ? context.User.Identity.Name
                : "anonymous";

            _logger.LogInformation("Incoming HTTP request {Method} {Path} from {User}", method, path, userName);

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                var statusCode = context.Response.StatusCode;

                var endpoint = context.GetEndpoint();
                var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
                var controllerName = actionDescriptor?.ControllerName ?? "UnknownController";

                _metrics.RegisterRequest(controllerName, stopwatch.ElapsedMilliseconds);

                _logger.LogInformation(
                    "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms (Controller={Controller})",
                    method,
                    path,
                    statusCode,
                    stopwatch.ElapsedMilliseconds,
                    controllerName);
            }
        }
    }
}
