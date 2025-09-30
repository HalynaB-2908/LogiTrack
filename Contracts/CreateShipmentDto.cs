namespace LogiTrack.WebApi.Contracts
{
    public class CreateShipmentDto
    {
        public string? Reference { get; set; }      
        public double DistanceKm { get; set; }      
        public double WeightKg { get; set; }        
    }
}

