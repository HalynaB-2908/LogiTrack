using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LogiTrack.WebApi.Contracts.Auth;
using LogiTrack.WebApi.Models;
using LogiTrack.WebApi.Services.Abstractions;

namespace LogiTrack.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly RoleManager<IdentityRole> _roles;
        private readonly ITokenService _tokens;
        private readonly ILogger<AuthController> _logger;

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
