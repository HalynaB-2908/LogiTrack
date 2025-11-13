using LogiTrack.WebApi.Options;
using LogiTrack.WebApi.Services.Abstractions;
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

        public IDeliveryTimeService CreateService(string? deliveryMode)
        {
            var mode = (deliveryMode ?? "standard").ToLowerInvariant();

            return mode switch
            {
                "express" => new ExpressDeliveryTimeService(),
                "eco" => new EcoDeliveryTimeService(),
                _ => new StandardDeliveryTimeService()
            };
        }
    }
}