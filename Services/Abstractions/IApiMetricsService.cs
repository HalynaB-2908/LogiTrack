using System.Collections.Generic;

namespace LogiTrack.WebApi.Services.Abstractions
{
    public interface IApiMetricsService
    {
        void RegisterRequest(string controllerName, long elapsedMilliseconds);
        long TotalRequests { get; }
        double AverageResponseTimeMs { get; }
        IReadOnlyDictionary<string, long> GetPerControllerCounts();
    }
}
