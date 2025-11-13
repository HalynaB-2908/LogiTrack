namespace LogiTrack.WebApi.Services.Abstractions
{
    public interface IDeliveryTimeService
    {
        double Estimate(double distanceKm);
    }
}