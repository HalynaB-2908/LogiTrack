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
    public class ShipmentsRepository : IShipmentsRepository
    {
        private readonly LogiTrackDbContext _db;

        public ShipmentsRepository(LogiTrackDbContext db)
        {
            _db = db;
        }

        public async Task<List<Shipment>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Shipments
                .Include(s => s.Customer)
                .Include(s => s.Vehicle)
                .AsNoTracking()
                .OrderBy(s => s.Id)
                .ToListAsync(ct);
        }

        public async Task<Shipment?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Shipments
                .Include(s => s.Customer)
                .Include(s => s.Vehicle)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id, ct);
        }

        public async Task<List<Shipment>> SearchAsync(string? query, ShipmentStatus? status, CancellationToken ct = default)
        {
            var q = _db.Shipments
                .Include(s => s.Customer)
                .Include(s => s.Vehicle)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
                q = q.Where(s => s.Reference.Contains(query));

            if (status.HasValue)
                q = q.Where(s => s.Status == status.Value);

            return await q.AsNoTracking().OrderBy(s => s.Id).ToListAsync(ct);
        }

        public async Task AddAsync(Shipment entity, CancellationToken ct = default)
        {
            await _db.Shipments.AddAsync(entity, ct);
        }

        public void Update(Shipment entity)
        {
            _db.Shipments.Update(entity);
        }

        public void Remove(Shipment entity)
        {
            _db.Shipments.Remove(entity);
        }
    }
}

