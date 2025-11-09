using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LogiTrack.WebApi.Models;

namespace LogiTrack.WebApi.Services.Abstractions
{
    public interface ICustomersRepository
    {
        Task<List<Customer>> GetAllAsync(CancellationToken ct = default);
        Task<Customer?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);

        Task AddAsync(Customer entity, CancellationToken ct = default);
        void Update(Customer entity);
        void Remove(Customer entity);
    }
}

