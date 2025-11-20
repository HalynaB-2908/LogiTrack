using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using LogiTrack.WebApi.Services.Abstractions;

namespace LogiTrack.WebApi.Services
{
    public class InMemoryApiMetricsService : IApiMetricsService
    {
        private long _totalRequests;
        private long _totalElapsedMilliseconds;

        private readonly ConcurrentDictionary<string, long> _perControllerCounts =
            new ConcurrentDictionary<string, long>();

        public void RegisterRequest(string controllerName, long elapsedMilliseconds)
        {
            Interlocked.Increment(ref _totalRequests);
            Interlocked.Add(ref _totalElapsedMilliseconds, elapsedMilliseconds);

            _perControllerCounts.AddOrUpdate(
                controllerName,
                1,
                (_, current) => current + 1);
        }

        public long TotalRequests => Interlocked.Read(ref _totalRequests);

        public double AverageResponseTimeMs
        {
            get
            {
                var total = Interlocked.Read(ref _totalRequests);
                var sum = Interlocked.Read(ref _totalElapsedMilliseconds);

                if (total == 0)
                    return 0.0;

                return (double)sum / total;
            }
        }

        public IReadOnlyDictionary<string, long> GetPerControllerCounts()
        {
            return new Dictionary<string, long>(_perControllerCounts);
        }
    }
}
