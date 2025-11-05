using LogiTrack.WebApi.Models;

namespace LogiTrack.WebApi.Contracts.Shipments
{
    public class ShipmentCreateUpdateDto
    {
        public string Reference { get; set; } = default!;
        public double DistanceKm { get; set; }
        public double WeightKg { get; set; }
        public int? CustomerId { get; set; }
        public int? VehicleId { get; set; }
    }

    public class ShipmentResponseDto
    {
        public int Id { get; set; }
        public string Reference { get; set; } = default!;
        public ShipmentStatus Status { get; set; }
        public double DistanceKm { get; set; }
        public double WeightKg { get; set; }
        public DateTime CreatedUtc { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? VehicleId { get; set; }
        public string? VehiclePlate { get; set; }
        public double EstimatedPrice { get; set; }
        public string Currency { get; set; } = default!;
    }
}
