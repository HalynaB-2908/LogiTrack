using LogiTrack.WebApi.Models;

namespace LogiTrack.WebApi.Repositories.Shipments
{
    public interface IShipmentsRepository
    {
        Task<IEnumerable<Shipment>> GetAllAsync();
        Task<Shipment?> GetByIdAsync(int id);
        Task<IEnumerable<Shipment>> SearchAsync(string? q, string? status);
        Task<Shipment> CreateAsync(Shipment shipment);
    }
}
