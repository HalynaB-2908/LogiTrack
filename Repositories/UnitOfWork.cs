using LogiTrack.WebApi.Data;
using LogiTrack.WebApi.Services.Abstractions;

namespace LogiTrack.WebApi.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LogiTrackDbContext _db;

        public IShipmentsRepository Shipments { get; }
        public ICustomersRepository Customers { get; }
        public IDriversRepository Drivers { get; }
        public IVehiclesRepository Vehicles { get; }

        public UnitOfWork(
            LogiTrackDbContext db,
            IShipmentsRepository shipments,
            ICustomersRepository customers,
            IDriversRepository drivers,
            IVehiclesRepository vehicles)
        {
            _db = db;
            Shipments = shipments;
            Customers = customers;
            Drivers = drivers;
            Vehicles = vehicles;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
