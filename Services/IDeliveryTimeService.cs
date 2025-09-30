namespace LogiTrack.WebApi.Services
{
    public interface IDeliveryTimeService
    {
        double Estimate(double distanceKm);
    }
}
