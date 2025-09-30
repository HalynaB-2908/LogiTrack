namespace LogiTrack.WebApi.Options
{
    public class LogisticsOptions
    {
        public string? CompanyName { get; set; }
        public string? DefaultShipmentStatus { get; set; }
        public double BasePricePerKm { get; set; }
        public double WeightPricePerKg { get; set; }
        public string? Currency { get; set; }
        public double AverageSpeedKmH { get; set; }
    }
}
