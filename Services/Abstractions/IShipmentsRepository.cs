using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LogiTrack.WebApi.Models;

namespace LogiTrack.WebApi.Services.Abstractions
{
    public interface IShipmentsRepository
    {
        Task<List<Shipment>> GetAllAsync(CancellationToken ct = default);
        Task<Shipment?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<List<Shipment>> SearchAsync(string? query, ShipmentStatus? status, CancellationToken ct = default);

        Task AddAsync(Shipment entity, CancellationToken ct = default);
        void Update(Shipment entity);
        void Remove(Shipment entity);
    }
}
