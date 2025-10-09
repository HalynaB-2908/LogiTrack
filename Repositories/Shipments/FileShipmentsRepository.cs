using System.Text;
using System.Text.Json;
using LogiTrack.WebApi.Models;

namespace LogiTrack.WebApi.Repositories.Shipments
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

        public FileShipmentsRepository(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<IEnumerable<Shipment>> GetAllAsync()
        {
            return await ReadAllAsync();
        }

        public async Task<Shipment?> GetByIdAsync(int id)
        {
            var items = await ReadAllAsync();
            return items.FirstOrDefault(s => s.Id == id);
        }

        public async Task<IEnumerable<Shipment>> SearchAsync(string? q, string? status)
        {
            var items = await ReadAllAsync();

            if (!string.IsNullOrWhiteSpace(q))
                items = items.Where(s => s.Reference.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(status))
                items = items.Where(s => s.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();

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
                if (string.IsNullOrWhiteSpace(shipment.Status)) shipment.Status = "Planned";

                items.Add(shipment);
                await WriteAllAsync(items);
                return shipment;
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<List<Shipment>> ReadAllAsync()
        {
            EnsureFileExists();

            using var fs = File.Open(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (fs.Length == 0) return new List<Shipment>();

            var list = await JsonSerializer.DeserializeAsync<List<Shipment>>(fs, _json);
            return list ?? new List<Shipment>();
        }

        private async Task WriteAllAsync(List<Shipment> items)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            var json = JsonSerializer.Serialize(items, _json);
            await File.WriteAllTextAsync(_filePath, json, Encoding.UTF8);
        }

        private void EnsureFileExists()
        {
            var dir = Path.GetDirectoryName(_filePath)!;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            if (!File.Exists(_filePath)) File.WriteAllText(_filePath, "[]", Encoding.UTF8);
        }
    }
}
