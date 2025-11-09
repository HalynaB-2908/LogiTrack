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
    public class CustomersRepository : ICustomersRepository
    {
        private readonly LogiTrackDbContext _db;

        public CustomersRepository(LogiTrackDbContext db)
        {
            _db = db;
        }

        public async Task<List<Customer>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Customers
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .ToListAsync(ct);
        }

        public async Task<Customer?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, ct);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        {
            return await _db.Customers.AnyAsync(c => c.Email == email, ct);
        }

        public async Task AddAsync(Customer entity, CancellationToken ct = default)
        {
            await _db.Customers.AddAsync(entity, ct);
        }

        public void Update(Customer entity)
        {
            _db.Customers.Update(entity);
        }

        public void Remove(Customer entity)
        {
            _db.Customers.Remove(entity);
        }
    }
}
