using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogiTrack.WebApi.Models
{
    public class Shipment
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Reference { get; set; } = default!;

        [Required]
        public ShipmentStatus Status { get; set; } = ShipmentStatus.Planned;

        public bool IsPaid { get; set; } = false;

        public double DistanceKm { get; set; }
        public double WeightKg { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? Price { get; set; }

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int? VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }
    }
}
