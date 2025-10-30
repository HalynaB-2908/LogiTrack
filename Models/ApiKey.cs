using System;
using System.ComponentModel.DataAnnotations;

namespace LogiTrack.WebApi.Models
{
    public class ApiKey
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Key { get; set; } = default!; 

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = default!; 

        public bool IsActive { get; set; } = true;   

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    }
}
