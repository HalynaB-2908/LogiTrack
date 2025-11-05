namespace LogiTrack.WebApi.Contracts.Vehicles
{
    public class VehicleCreateUpdateDto
    {
        public string PlateNumber { get; set; } = default!;
        public string Model { get; set; } = default!;
        public double CapacityKg { get; set; }
        public int? DriverId { get; set; }
    }

    public class VehicleResponseDto
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; } = default!;
        public string Model { get; set; } = default!;
        public double CapacityKg { get; set; }
        public int? DriverId { get; set; }
        public string? DriverName { get; set; }
    }
}