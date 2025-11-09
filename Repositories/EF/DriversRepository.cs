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
    public class DriversRepository : IDriversRepository
    {
        private readonly LogiTrackDbContext _db;

        public DriversRepository(LogiTrackDbContext db)
        {
            _db = db;
        }

        public async Task<List<Driver>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Drivers
                .AsNoTracking()
                .OrderBy(d => d.Id)
                .ToListAsync(ct);
        }

        public async Task<Driver?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Drivers
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id, ct);
        }

        public async Task AddAsync(Driver entity, CancellationToken ct = default)
        {
            await _db.Drivers.AddAsync(entity, ct);
        }

        public void Update(Driver entity)
        {
            _db.Drivers.Update(entity);
        }

        public void Remove(Driver entity)
        {
            _db.Drivers.Remove(entity);
        }
    }
}
