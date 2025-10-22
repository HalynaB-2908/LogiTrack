using LogiTrack.WebApi.Models;

namespace LogiTrack.WebApi.Repositories.Shipments
{
    public class InMemoryShipmentsRepository : IShipmentsRepository
    {
        private readonly List<Shipment> _shipments = new();
        private int _nextId = 1;

        public Task<IEnumerable<Shipment>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<Shipment>>(_shipments);
        }

        public Task<Shipment?> GetByIdAsync(int id)
        {
            var shipment = _shipments.FirstOrDefault(s => s.Id == id);
            return Task.FromResult(shipment);
        }

        public Task<IEnumerable<Shipment>> SearchAsync(string? q, ShipmentStatus? status)
        {
            var results = _shipments.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(q))
                results = results.Where(s =>
                    s.Reference.Contains(q, StringComparison.OrdinalIgnoreCase));

            if (status.HasValue)
                results = results.Where(s => s.Status == status.Value);

            return Task.FromResult(results);
        }

        public Task<Shipment> CreateAsync(Shipment shipment)
        {
            shipment.Id = _nextId++;

            if (!Enum.IsDefined(typeof(ShipmentStatus), shipment.Status))
                shipment.Status = ShipmentStatus.Planned;

            _shipments.Add(shipment);
            return Task.FromResult(shipment);
        }
    }
}

