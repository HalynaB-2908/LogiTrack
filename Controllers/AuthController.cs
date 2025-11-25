using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LogiTrack.WebApi.Contracts.Auth;
using LogiTrack.WebApi.Models;
using LogiTrack.WebApi.Services.Abstractions;

namespace LogiTrack.WebApi.Controllers
{
    /// <summary>
    /// Controller responsible for user authentication and authorization operations.
    /// Provides endpoints for user registration, login and retrieval of current user claims.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly RoleManager<IdentityRole> _roles;
        private readonly ITokenService _tokens;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// Injects required services for user and role management, token generation and logging.
        /// </summary>
        /// <param name="users">User manager for managing application users.</param>
        /// <param name="roles">Role manager responsible for role operations.</param>
        /// <param name="tokens">Service for generating JWT tokens.</param>
        /// <param name="logger">Logger instance for authentication events.</param>
        public AuthController(
            UserManager<ApplicationUser> users,
            RoleManager<IdentityRole> roles,
            ITokenService tokens,
            ILogger<AuthController> logger)
        {
            _users = users;
            _roles = roles;
            _tokens = tokens;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user in the system and assigns the specified role.
        /// After successful registration, a JWT token is generated and returned.
        /// </summary>
        /// <param name="dto">Registration data including email, password, username and role.</param>
        /// <returns>Authentication response with user info and JWT token.</returns>
        /// <response code="200">User successfully registered.</response>
        /// <response code="400">Registration failed due to validation errors.</response>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            _logger.LogInformation("Registration attempt for email {Email}", dto.Email);

            var user = new ApplicationUser
            {
                Email = dto.Email,
                UserName = string.IsNullOrWhiteSpace(dto.UserName) ? dto.Email : dto.UserName
            };

            var result = await _users.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Registration failed for email {Email}", dto.Email);
                return BadRequest(result.Errors);
            }

            if (!await _roles.RoleExistsAsync(dto.Role))
            {
                await _roles.CreateAsync(new IdentityRole(dto.Role));
                _logger.LogInformation("Role {Role} created", dto.Role);
            }

            await _users.AddToRoleAsync(user, dto.Role);

            var roles = await _users.GetRolesAsync(user);
            var tokenString = await _tokens.CreateTokenAsync(user);

            _logger.LogInformation("User {Email} successfully registered", dto.Email);

            return Ok(new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email!,
                UserName = user.UserName,
                Token = tokenString,
                Roles = roles.ToArray()
            });
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token if credentials are valid.
        /// </summary>
        /// <param name="dto">Login credentials (email or username and password).</param>
        /// <returns>Authentication response with JWT token.</returns>
        /// <response code="200">Login successful.</response>
        /// <response code="401">Invalid credentials.</response>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            _logger.LogInformation("Login attempt for {User}", dto.EmailOrUserName);

            ApplicationUser? user =
                await _users.FindByEmailAsync(dto.EmailOrUserName)
                ?? await _users.FindByNameAsync(dto.EmailOrUserName);

            if (user == null)
            {
                _logger.LogWarning("Login failed: user {User} not found", dto.EmailOrUserName);
                return Unauthorized("Invalid credentials.");
            }

            if (!await _users.CheckPasswordAsync(user, dto.Password))
            {
                _logger.LogWarning("Login failed: invalid password for {User}", dto.EmailOrUserName);
                return Unauthorized("Invalid credentials.");
            }

            var roles = await _users.GetRolesAsync(user);
            var tokenString = await _tokens.CreateTokenAsync(user);

            _logger.LogInformation("User {User} successfully logged in", dto.EmailOrUserName);

            return Ok(new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email!,
                UserName = user.UserName,
                Token = tokenString,
                Roles = roles.ToArray()
            });
        }

        /// <summary>
        /// Returns information about the currently authenticated user based on JWT claims.
        /// </summary>
        /// <returns>List of claims associated with the current user.</returns>
        /// <response code="200">Claims successfully returned.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            _logger.LogInformation("User {User} requested profile info", User.Identity?.Name);
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(claims);
        }
    }
}
