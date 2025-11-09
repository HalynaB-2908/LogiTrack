using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LogiTrack.WebApi.Data;
using LogiTrack.WebApi.Models;
using LogiTrack.WebApi.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.WebApi.Repositories.EF
{
    public class VehiclesRepository : IVehiclesRepository
    {
        private readonly LogiTrackDbContext _db;

        public VehiclesRepository(LogiTrackDbContext db)
        {
            _db = db;
        }

        public async Task<List<Vehicle>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Vehicles
                .Include(v => v.Driver)
                .AsNoTracking()
                .OrderBy(v => v.Id)
                .ToListAsync(ct);
        }

        public async Task<Vehicle?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Vehicles
                .Include(v => v.Driver)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id, ct);
        }

        public async Task<bool> ExistsByPlateAsync(string plateNumber, CancellationToken ct = default)
        {
            return await _db.Vehicles.AnyAsync(v => v.PlateNumber == plateNumber, ct);
        }

        public async Task AddAsync(Vehicle entity, CancellationToken ct = default)
        {
            await _db.Vehicles.AddAsync(entity, ct);
        }

        public void Update(Vehicle entity)
        {
            _db.Vehicles.Update(entity);
        }

        public void Remove(Vehicle entity)
        {
            _db.Vehicles.Remove(entity);
        }
    }
}
