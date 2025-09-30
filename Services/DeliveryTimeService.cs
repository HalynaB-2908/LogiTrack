using LogiTrack.WebApi.Options;
using Microsoft.Extensions.Options;

namespace LogiTrack.WebApi.Services
{
    public class DeliveryTimeService : IDeliveryTimeService
    {
        private readonly double _avgSpeedKmH;

        public DeliveryTimeService(IOptions<LogisticsOptions> options)
        {
            _avgSpeedKmH = options.Value.AverageSpeedKmH > 0
                ? options.Value.AverageSpeedKmH
                : 60; // fallback
        }

        public double Estimate(double distanceKm)
        {
            if (distanceKm <= 0) return 0;
            return distanceKm / _avgSpeedKmH;
        }
    }
}

