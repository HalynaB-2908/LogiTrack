using LogiTrack.WebApi.Options;
using LogiTrack.WebApi.Services.Abstractions;
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
                : 60;
        }

        public double Estimate(double distanceKm)
        {
            if (distanceKm <= 0) return 0;
            return distanceKm / _avgSpeedKmH;
        }
    }
}

