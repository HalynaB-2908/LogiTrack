using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LogiTrack.WebApi.Models;

namespace LogiTrack.WebApi.Services.Abstractions
{
    public interface IVehiclesRepository
    {
        Task<List<Vehicle>> GetAllAsync(CancellationToken ct = default);
        Task<Vehicle?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<bool> ExistsByPlateAsync(string plateNumber, CancellationToken ct = default);

        Task AddAsync(Vehicle entity, CancellationToken ct = default);
        void Update(Vehicle entity);
        void Remove(Vehicle entity);
    }
}
