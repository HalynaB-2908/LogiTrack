using LogiTrack.WebApi.Services.Abstractions;
using LogiTrack.WebApi.Options;
using Microsoft.Extensions.Options;

namespace LogiTrack.WebApi.Services.Factories
{
    public class DeliveryTimeServiceFactory
    {
        private readonly LogisticsOptions _options;

        public DeliveryTimeServiceFactory(IOptions<LogisticsOptions> options)
        {
            _options = options.Value;
        }

        public IDeliveryTimeService CreateService()
        {
            return _options.Mode?.ToLower() switch
            {
                "express" => new ExpressDeliveryTimeService(),
                "eco" => new EcoDeliveryTimeService(),
                _ => new StandardDeliveryTimeService()
            };
        }
    }
   
    public class StandardDeliveryTimeService : IDeliveryTimeService
    {
        public double CalculateHours(double distanceKm)
        {
            return distanceKm / 60.0; 
        }
    }

    public class ExpressDeliveryTimeService : IDeliveryTimeService
    {
        public double CalculateHours(double distanceKm)
        {
            return distanceKm / 100.0; 
        }
    }

    public class EcoDeliveryTimeService : IDeliveryTimeService
    {
        public double CalculateHours(double distanceKm)
        {
            return distanceKm / 40.0; 
        }
    }
}
