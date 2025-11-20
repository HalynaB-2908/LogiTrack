using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LogiTrack.WebApi.Models;
using LogiTrack.WebApi.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace LogiTrack.WebApi.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _cfg;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TokenService> _logger;

        public TokenService(
            IConfiguration cfg,
            UserManager<ApplicationUser> userManager,
            ILogger<TokenService> logger)
        {
            _cfg = cfg;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<string> CreateTokenAsync(ApplicationUser user)
        {
            var jwt = _cfg.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
            };

            var roles = await _userManager.GetRolesAsync(user);

            _logger.LogInformation(
                "Generating JWT for user {UserName} with roles {Roles}",
                user.UserName ?? user.Email ?? "unknown",
                string.Join(",", roles));

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpiresMinutes"]!));

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            _logger.LogInformation(
                "JWT generated successfully for user {UserName}. Expires at {ExpiresUtc}",
                user.UserName ?? user.Email ?? "unknown",
                expires);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
