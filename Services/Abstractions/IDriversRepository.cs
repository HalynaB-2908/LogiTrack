using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LogiTrack.WebApi.Models;

namespace LogiTrack.WebApi.Services.Abstractions
{
    public interface IDriversRepository
    {
        Task<List<Driver>> GetAllAsync(CancellationToken ct = default);
        Task<Driver?> GetByIdAsync(int id, CancellationToken ct = default);

        Task AddAsync(Driver entity, CancellationToken ct = default);
        void Update(Driver entity);
        void Remove(Driver entity);
    }
}
