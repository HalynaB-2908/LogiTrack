using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LogiTrack.WebApi.Models;
using LogiTrack.WebApi.Services.Abstractions;

namespace LogiTrack.WebApi.Repositories.File
{
    public class InMemoryShipmentsRepository : IShipmentsRepository
    {
        private readonly List<Shipment> _shipments = new();
        private int _nextId = 1;

        public Task<List<Shipment>> GetAllAsync(CancellationToken ct = default)
        {
            var copy = _shipments.ToList();
            return Task.FromResult(copy);
        }

        public Task<Shipment?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var shipment = _shipments.FirstOrDefault(s => s.Id == id);
            return Task.FromResult(shipment);
        }

        public Task<List<Shipment>> SearchAsync(string? query, ShipmentStatus? status, CancellationToken ct = default)
        {
            IEnumerable<Shipment> result = _shipments;

            if (!string.IsNullOrWhiteSpace(query))
            {
                result = result.Where(s =>
                    s.Reference.Contains(query, StringComparison.OrdinalIgnoreCase));
            }

            if (status.HasValue)
            {
                result = result.Where(s => s.Status == status.Value);
            }

            return Task.FromResult(result.ToList());
        }

        public Task AddAsync(Shipment entity, CancellationToken ct = default)
        {
            entity.Id = _nextId++;

            if (!Enum.IsDefined(typeof(ShipmentStatus), entity.Status))
            {
                entity.Status = ShipmentStatus.Planned;
            }

            if (entity.CreatedUtc == default)
            {
                entity.CreatedUtc = DateTime.UtcNow;
            }

            _shipments.Add(entity);
            return Task.CompletedTask;
        }

        public void Update(Shipment entity)
        {
            var existing = _shipments.FirstOrDefault(s => s.Id == entity.Id);
            if (existing == null) return;

            existing.Reference = entity.Reference;
            existing.Status = entity.Status;
            existing.DistanceKm = entity.DistanceKm;
            existing.WeightKg = entity.WeightKg;
            existing.CustomerId = entity.CustomerId;
            existing.VehicleId = entity.VehicleId;
            existing.CreatedUtc = entity.CreatedUtc;
        }

        public void Remove(Shipment entity)
        {
            _shipments.RemoveAll(s => s.Id == entity.Id);
        }
    }
}