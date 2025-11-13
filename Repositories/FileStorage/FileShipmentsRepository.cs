using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using LogiTrack.WebApi.Models;
using LogiTrack.WebApi.Services.Abstractions;
using IO = System.IO;

namespace LogiTrack.WebApi.Repositories.File
{
    public class FileShipmentsRepository : IShipmentsRepository
    {
        private readonly string _filePath;
        private readonly SemaphoreSlim _lock = new(1, 1);

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        static FileShipmentsRepository()
        {
            _json.Converters.Add(new JsonStringEnumConverter());
        }

        public FileShipmentsRepository(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<List<Shipment>> GetAllAsync(CancellationToken ct = default)
        {
            var items = await ReadAllAsync(ct);
            return items;
        }

        public async Task<Shipment?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var items = await ReadAllAsync(ct);
            return items.FirstOrDefault(s => s.Id == id);
        }

        public async Task<List<Shipment>> SearchAsync(string? query, ShipmentStatus? status, CancellationToken ct = default)
        {
            var items = await ReadAllAsync(ct);
            IEnumerable<Shipment> result = items;

            if (!string.IsNullOrWhiteSpace(query))
            {
                result = result.Where(s =>
                    s.Reference.Contains(query, StringComparison.OrdinalIgnoreCase));
            }

            if (status.HasValue)
            {
                result = result.Where(s => s.Status == status.Value);
            }

            return result.OrderBy(s => s.Id).ToList();
        }

        public async Task AddAsync(Shipment entity, CancellationToken ct = default)
        {
            await _lock.WaitAsync(ct);
            try
            {
                var items = await ReadAllAsync(ct);
                var nextId = items.Count == 0 ? 1 : items.Max(s => s.Id) + 1;

                entity.Id = nextId;

                if (!Enum.IsDefined(typeof(ShipmentStatus), entity.Status))
                {
                    entity.Status = ShipmentStatus.Planned;
                }

                if (entity.CreatedUtc == default)
                {
                    entity.CreatedUtc = DateTime.UtcNow;
                }

                items.Add(entity);
                await WriteAllAsync(items, ct);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async void Update(Shipment entity)
        {
            await _lock.WaitAsync();
            try
            {
                var items = await ReadAllAsync();
                var index = items.FindIndex(s => s.Id == entity.Id);
                if (index == -1) return;

                items[index] = entity;
                await WriteAllAsync(items);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async void Remove(Shipment entity)
        {
            await _lock.WaitAsync();
            try
            {
                var items = await ReadAllAsync();
                items.RemoveAll(s => s.Id == entity.Id);
                await WriteAllAsync(items);
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<List<Shipment>> ReadAllAsync(CancellationToken ct = default)
        {
            EnsureFileExists();

            await using var fs = IO.File.Open(_filePath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read);
            if (fs.Length == 0) return new List<Shipment>();

            var list = await JsonSerializer.DeserializeAsync<List<Shipment>>(fs, _json, ct);
            return list ?? new List<Shipment>();
        }

        private async Task WriteAllAsync(List<Shipment> items, CancellationToken ct = default)
        {
            var dir = IO.Path.GetDirectoryName(_filePath)!;
            if (!IO.Directory.Exists(dir))
            {
                IO.Directory.CreateDirectory(dir);
            }

            var json = JsonSerializer.Serialize(items, _json);
            await IO.File.WriteAllTextAsync(_filePath, json, Encoding.UTF8, ct);
        }

        private void EnsureFileExists()
        {
            var dir = IO.Path.GetDirectoryName(_filePath)!;
            if (!IO.Directory.Exists(dir)) IO.Directory.CreateDirectory(dir);
            if (!IO.File.Exists(_filePath)) IO.File.WriteAllText(_filePath, "[]", Encoding.UTF8);
        }
    }
}