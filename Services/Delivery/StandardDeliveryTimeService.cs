using LogiTrack.WebApi.Services.Abstractions;

namespace LogiTrack.WebApi.Services
{
    public class StandardDeliveryTimeService : IDeliveryTimeService
    {
        public double Estimate(double distanceKm)
        {
            if (distanceKm <= 0)
                return 0;

            return distanceKm / 80.0;
        }
    }
}