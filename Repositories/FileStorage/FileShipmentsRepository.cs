using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public FileShipmentsRepository(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<IEnumerable<Shipment>> GetAllAsync() =>
            await ReadAllAsync();

        public async Task<Shipment?> GetByIdAsync(int id)
        {
            var items = await ReadAllAsync();
            return items.FirstOrDefault(s => s.Id == id);
        }

        public async Task<IEnumerable<Shipment>> SearchAsync(string? q, ShipmentStatus? status)
        {
            var items = await ReadAllAsync();

            if (!string.IsNullOrWhiteSpace(q))
                items = items.Where(s =>
                    s.Reference.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

            if (status.HasValue)
                items = items.Where(s => s.Status == status.Value).ToList();

            return items;
        }

        public async Task<Shipment> CreateAsync(Shipment shipment)
        {
            await _lock.WaitAsync();
            try
            {
                var items = await ReadAllAsync();
                var nextId = items.Count == 0 ? 1 : items.Max(s => s.Id) + 1;

                shipment.Id = nextId;
                if (shipment.CreatedUtc == default) shipment.CreatedUtc = DateTime.UtcNow;

                if (!Enum.IsDefined(typeof(ShipmentStatus), shipment.Status))
                    shipment.Status = ShipmentStatus.Planned;

                items.Add(shipment);
                await WriteAllAsync(items);
                return shipment;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _lock.WaitAsync();
            try
            {
                var items = await ReadAllAsync();
                var removed = items.RemoveAll(s => s.Id == id) > 0;
                if (removed) await WriteAllAsync(items);
                return removed;
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<List<Shipment>> ReadAllAsync()
        {
            EnsureFileExists();

            using var fs = IO.File.Open(_filePath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read);
            if (fs.Length == 0) return new List<Shipment>();

            var list = await JsonSerializer.DeserializeAsync<List<Shipment>>(fs, _json);
            return list ?? new List<Shipment>();
        }

        private async Task WriteAllAsync(List<Shipment> items)
        {
            var dir = IO.Path.GetDirectoryName(_filePath)!;
            IO.Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(items, _json);
            await IO.File.WriteAllTextAsync(_filePath, json, Encoding.UTF8);
        }

        private void EnsureFileExists()
        {
            var dir = IO.Path.GetDirectoryName(_filePath)!;
            if (!IO.Directory.Exists(dir)) IO.Directory.CreateDirectory(dir);
            if (!IO.File.Exists(_filePath)) IO.File.WriteAllText(_filePath, "[]", Encoding.UTF8);
        }
    }
}