using LogiTrack.WebApi.Services.Abstractions;

namespace LogiTrack.WebApi.Services
{
    public class EcoDeliveryTimeService : IDeliveryTimeService
    {
        public double Estimate(double distanceKm)
        {
            if (distanceKm <= 0)
                return 0;

            return distanceKm / 40.0;
        }
    }
}