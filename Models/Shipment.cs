namespace LogiTrack.WebApi.Models
{
    public class Shipment
    {
        public int Id { get; set; }
        public string Reference { get; set; } = "";
        public string Status { get; set; } = "Planned";
        public double DistanceKm { get; set; }
        public double WeightKg { get; set; }
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    }
}
