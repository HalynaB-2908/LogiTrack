using System.Threading;
using System.Threading.Tasks;

namespace LogiTrack.WebApi.Services.Abstractions
{
    public interface IDeliveryTimeService
    {
        Task<double> EstimateAsync(double distanceKm, CancellationToken ct = default);
    }
}
