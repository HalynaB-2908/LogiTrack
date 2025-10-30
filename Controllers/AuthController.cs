using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LogiTrack.WebApi.Contracts.Auth;
using LogiTrack.WebApi.Models;
using LogiTrack.WebApi.Services;
using System.IdentityModel.Tokens.Jwt;

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

        public AuthController(
            UserManager<ApplicationUser> users,
            RoleManager<IdentityRole> roles,
            ITokenService tokens)
        {
            _users = users;
            _roles = roles;
            _tokens = tokens;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var user = new ApplicationUser
            {
                Email = dto.Email,
                UserName = string.IsNullOrWhiteSpace(dto.UserName) ? dto.Email : dto.UserName
            };

            var result = await _users.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            if (!await _roles.RoleExistsAsync(dto.Role))
                await _roles.CreateAsync(new IdentityRole(dto.Role));

            await _users.AddToRoleAsync(user, dto.Role);

            var roles = await _users.GetRolesAsync(user);
            var tokenString = await _tokens.CreateTokenAsync(user);

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
            ApplicationUser? user =
                await _users.FindByEmailAsync(dto.EmailOrUserName)
                ?? await _users.FindByNameAsync(dto.EmailOrUserName);

            if (user == null)
                return Unauthorized("Invalid credentials.");

            if (!await _users.CheckPasswordAsync(user, dto.Password))
                return Unauthorized("Invalid credentials.");

            var roles = await _users.GetRolesAsync(user);
            var tokenString = await _tokens.CreateTokenAsync(user);

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
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(claims);
        }
    }
}
