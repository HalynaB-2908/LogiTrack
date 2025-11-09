using System.Threading;
using System.Threading.Tasks;

namespace LogiTrack.WebApi.Services.Abstractions
{
    public interface IUnitOfWork
    {
        IShipmentsRepository Shipments { get; }
        ICustomersRepository Customers { get; }
        IDriversRepository Drivers { get; }
        IVehiclesRepository Vehicles { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

