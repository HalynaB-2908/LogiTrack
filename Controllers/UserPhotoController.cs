using LogiTrack.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace LogiTrack.WebApi.Controllers
{
    /// <summary>
    /// Controller responsible for uploading user profile photos.
    /// Allows an authenticated user or administrator to upload a photo for a specific user.
    /// </summary>
    [ApiController]
    [Route("api/v1/users/{userId}/photo")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin,User")]
    public class UserPhotoController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly ILogger<UserPhotoController> _logger;

        private static readonly Serilog.ILogger UploadLogger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                path: "Logs/uploads-log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                shared: true
            )
            .CreateLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPhotoController"/> class.
        /// </summary>
        /// <param name="users">User manager for accessing application users.</param>
        /// <param name="logger">Logger for controller diagnostics.</param>
        public UserPhotoController(
            UserManager<ApplicationUser> users,
            ILogger<UserPhotoController> logger)
        {
            _users = users;
            _logger = logger;
        }

        /// <summary>
        /// Uploads a profile photo for the specified user.
        /// The photo is saved on the server in the user's upload directory.
        /// </summary>
        /// <param name="userId">Identifier of the user whose photo is being uploaded.</param>
        /// <param name="file">Image file sent through multipart/form-data.</param>
        /// <returns>Result of the upload operation.</returns>
        /// <response code="200">Photo uploaded successfully.</response>
        /// <response code="400">Invalid input data or empty file.</response>
        /// <response code="404">User not found.</response>
        /// <response code="500">Internal server error during file upload.</response>
        [HttpPost]
        [RequestSizeLimit(10_000_000)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadPhoto(
            [FromRoute] string userId,
            IFormFile file)
        {
            var currentUser = User?.Identity?.IsAuthenticated == true
                ? User.Identity!.Name
                : "anonymous";

            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Photo upload: empty userId from {CurrentUser}", currentUser);
                UploadLogger.Warning("Photo upload: empty userId from {CurrentUser}", currentUser);
                return BadRequest("UserId is required.");
            }

            var user = await _users.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning(
                    "Photo upload failed: user {UserId} not found (requested by {CurrentUser})",
                    userId,
                    currentUser);

                UploadLogger.Warning(
                    "Photo upload failed: user {UserId} not found (requested by {CurrentUser})",
                    userId,
                    currentUser);

                return NotFound("User not found.");
            }

            if (file == null || file.Length == 0)
            {
                _logger.LogWarning(
                    "Photo upload failed: empty file for user {UserId} by {CurrentUser}",
                    userId,
                    currentUser);

                UploadLogger.Warning(
                    "Photo upload failed: empty file for user {UserId} by {CurrentUser}",
                    userId,
                    currentUser);

                return BadRequest("File is empty.");
            }

            try
            {
                _logger.LogInformation(
                    "Photo upload started for user {UserId}: {FileName} ({SizeBytes} bytes) by {CurrentUser}",
                    userId,
                    file.FileName,
                    file.Length,
                    currentUser);

                UploadLogger.Information(
                    "Photo upload started for user {UserId}: {FileName} ({SizeBytes} bytes) by {CurrentUser}",
                    userId,
                    file.FileName,
                    file.Length,
                    currentUser);

                var uploadsRoot = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Uploads",
                    "Users",
                    userId);

                Directory.CreateDirectory(uploadsRoot);

                var safeFileName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(uploadsRoot, safeFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation(
                    "Photo upload succeeded for user {UserId}: {FileName}",
                    userId,
                    safeFileName);

                UploadLogger.Information(
                    "Photo upload succeeded for user {UserId}: {FileName}",
                    userId,
                    safeFileName);

                return Ok(new
                {
                    Message = "Photo uploaded successfully.",
                    UserId = userId,
                    FileName = safeFileName,
                    SizeBytes = file.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Photo upload failed for user {UserId} by {CurrentUser}",
                    userId,
                    currentUser);

                UploadLogger.Error(
                    ex,
                    "Photo upload failed for user {UserId} by {CurrentUser}",
                    userId,
                    currentUser);

                return StatusCode(StatusCodes.Status500InternalServerError, "Photo upload failed.");
            }
        }
    }
}
