using LogiTrack.WebApi.Data;
using LogiTrack.WebApi.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LogiTrack.WebApi.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LogiTrackDbContext _db;
        private readonly ILogger<UnitOfWork> _logger;

        public IShipmentsRepository Shipments { get; }
        public ICustomersRepository Customers { get; }
        public IDriversRepository Drivers { get; }
        public IVehiclesRepository Vehicles { get; }

        public UnitOfWork(
            LogiTrackDbContext db,
            IShipmentsRepository shipments,
            ICustomersRepository customers,
            IDriversRepository drivers,
            IVehiclesRepository vehicles,
            ILogger<UnitOfWork> logger)
        {
            _db = db;
            Shipments = shipments;
            Customers = customers;
            Drivers = drivers;
            Vehicles = vehicles;
            _logger = logger;
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            try
            {
                return await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error while saving changes.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected database error while saving changes.");
                throw;
            }
        }
    }
}
