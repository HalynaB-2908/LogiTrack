using LogiTrack.WebApi.Options;
using LogiTrack.WebApi.Services.Abstractions;
using Microsoft.Extensions.Options;

namespace LogiTrack.WebApi.Services.Factories
{
    public class DeliveryTimeServiceFactory
    {
        private readonly LogisticsOptions _options;
        private readonly ILogger<DeliveryTimeServiceFactory> _logger;

        public DeliveryTimeServiceFactory(
            IOptions<LogisticsOptions> options,
            ILogger<DeliveryTimeServiceFactory> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public IDeliveryTimeService CreateService(string? deliveryMode)
        {
            var mode = (deliveryMode ?? "standard").Trim().ToLowerInvariant();

            _logger.LogInformation("DeliveryTimeServiceFactory: requested mode '{Mode}'", mode);

            switch (mode)
            {
                case "express":
                    _logger.LogInformation("Using ExpressDeliveryTimeService for mode '{Mode}'", mode);
                    return new ExpressDeliveryTimeService();

                case "eco":
                case "economy":
                    _logger.LogInformation("Using EcoDeliveryTimeService for mode '{Mode}'", mode);
                    return new EcoDeliveryTimeService();

                case "standard":
                case "":
                    _logger.LogInformation("Using StandardDeliveryTimeService for mode '{Mode}'", mode);
                    return new StandardDeliveryTimeService();

                default:
                    _logger.LogWarning(
                        "Unknown delivery mode '{Mode}'. Falling back to StandardDeliveryTimeService.",
                        mode);

                    return new StandardDeliveryTimeService();
            }
        }
    }
}
