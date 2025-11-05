namespace LogiTrack.WebApi.Contracts.Drivers
{
    public class DriverCreateUpdateDto
    {
        public string FullName { get; set; } = default!;
        public string? Phone { get; set; }
    }

    public class DriverResponseDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = default!;
        public string? Phone { get; set; }
    }
}