using System.ComponentModel.DataAnnotations;

namespace LogiTrack.WebApi.Models
{
    public class Vehicle
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string PlateNumber { get; set; } = default!;

        [Required, MaxLength(200)]
        public string Model { get; set; } = default!;

        public double CapacityKg { get; set; }

        public int? DriverId { get; set; }
        public Driver? Driver { get; set; }

        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    }
}
