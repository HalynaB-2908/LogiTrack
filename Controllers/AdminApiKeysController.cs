using System.Security.Cryptography;
using System.Text;
using LogiTrack.WebApi.Data;
using LogiTrack.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/admin/apikeys")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin")] 
    public class AdminApiKeysController : ControllerBase
    {
        private readonly LogiTrackDbContext _db;

        public AdminApiKeysController(LogiTrackDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.ApiKeys
                .AsNoTracking()
                .Select(k => new
                {
                    k.Id,
                    k.Name,
                    k.IsActive,
                    k.CreatedUtc
                })
                .ToListAsync();

            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateApiKeyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Name is required.");

            var rawBytes = RandomNumberGenerator.GetBytes(32);
            var generatedKey = "LT_API_" + Convert.ToBase64String(rawBytes);

            var entity = new ApiKey
            {
                Name = dto.Name.Trim(),
                Key = generatedKey,
                IsActive = true,
                CreatedUtc = DateTime.UtcNow
            };

            _db.ApiKeys.Add(entity);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                entity.Id,
                entity.Name,
                entity.IsActive,
                entity.CreatedUtc,
                ApiKey = entity.Key
            });
        }

        [HttpPatch("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate([FromRoute] int id)
        {
            var key = await _db.ApiKeys.FirstOrDefaultAsync(k => k.Id == id);
            if (key == null) return NotFound("API key not found.");

            key.IsActive = false;
            await _db.SaveChangesAsync();

            return Ok(new { message = "API key deactivated." });
        }
    }

    public class CreateApiKeyDto
    {
        public string Name { get; set; } = string.Empty;
    }
}
