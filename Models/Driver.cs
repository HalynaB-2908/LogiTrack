using System.ComponentModel.DataAnnotations;

namespace LogiTrack.WebApi.Models
{
    public class Driver
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string FullName { get; set; } = default!;

        [MaxLength(50)]
        public string? Phone { get; set; }

        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}

