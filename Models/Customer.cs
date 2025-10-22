using System.ComponentModel.DataAnnotations;

namespace LogiTrack.WebApi.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = default!;

        [Required, MaxLength(200), EmailAddress]
        public string Email { get; set; } = default!;

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    }
}

