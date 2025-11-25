using System.Security.Cryptography;
using LogiTrack.WebApi.Data;
using LogiTrack.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.WebApi.Controllers
{
    /// <summary>
    /// Controller for administrative management of API keys.
    /// Provides functionality to view, create and deactivate API keys.
    /// Accessible only for users with Admin role.
    /// </summary>
    [ApiController]
    [Route("api/v1/admin/apikeys")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin")]
    public class AdminApiKeysController : ControllerBase
    {
        private readonly LogiTrackDbContext _db;
        private readonly ILogger<AdminApiKeysController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminApiKeysController"/> class.
        /// </summary>
        /// <param name="db">Database context for accessing API keys.</param>
        /// <param name="logger">Logger instance for diagnostic and audit messages.</param>
        public AdminApiKeysController(LogiTrackDbContext db, ILogger<AdminApiKeysController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Returns a list of all API keys with basic information.
        /// Sensitive key values are not included in this response.
        /// </summary>
        /// <returns>List of API keys.</returns>
        /// <response code="200">API keys list successfully returned.</response>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Admin requested list of all API keys.");

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

            _logger.LogInformation("Admin API keys list returned. Count={ApiKeysCount}", list.Count);

            return Ok(list);
        }

        /// <summary>
        /// Creates a new API key with a generated secure token.
        /// </summary>
        /// <param name="dto">Object containing the name of the API key.</param>
        /// <returns>Created API key details including generated token.</returns>
        /// <response code="200">API key successfully created.</response>
        /// <response code="400">Name is missing or invalid.</response>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateApiKeyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                _logger.LogWarning("Attempt to create API key with empty or whitespace Name.");
                return BadRequest("Name is required.");
            }

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

            _logger.LogInformation(
                "API key created. Id={ApiKeyId}, Name={ApiKeyName}, IsActive={IsActive}",
                entity.Id,
                entity.Name,
                entity.IsActive
            );

            return Ok(new
            {
                entity.Id,
                entity.Name,
                entity.IsActive,
                entity.CreatedUtc,
                ApiKey = entity.Key
            });
        }

        /// <summary>
        /// Deactivates an existing API key by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the API key.</param>
        /// <returns>Status message indicating the result.</returns>
        /// <response code="200">API key successfully deactivated.</response>
        /// <response code="404">API key not found.</response>
        [HttpPatch("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate([FromRoute] int id)
        {
            var key = await _db.ApiKeys.FirstOrDefaultAsync(k => k.Id == id);
            if (key == null)
            {
                _logger.LogWarning("Attempt to deactivate non-existing API key. Id={ApiKeyId}", id);
                return NotFound("API key not found.");
            }

            if (!key.IsActive)
            {
                _logger.LogInformation("Attempt to deactivate already inactive API key. Id={ApiKeyId}", id);
            }

            key.IsActive = false;
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "API key deactivated. Id={ApiKeyId}, Name={ApiKeyName}",
                key.Id,
                key.Name
            );

            return Ok(new { message = "API key deactivated." });
        }
    }

    /// <summary>
    /// DTO used for creating a new API key.
    /// </summary>
    public class CreateApiKeyDto
    {
        /// <summary>
        /// Name of the API key.
        /// Used for identification and management purposes.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
