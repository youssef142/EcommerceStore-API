using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EcommerceStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace EcommerceStore.Infrastructure
{
    public sealed class TokenService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _users;

        public TokenService(IConfiguration config, UserManager<ApplicationUser> users)
        {
            _config = config;
            _users = users;
        }

        public async Task<(string Token, DateTime ExpiresAt)> CreateAccessTokenAsync(ApplicationUser user)
        {
            var roles = await _users.GetRolesAsync(user);

            // ClaimTypes.* on purpose: existing code (CartService, controllers) already reads
            // ClaimTypes.NameIdentifier and User.IsInRole("Admin").
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var minutes = _config.GetValue<int?>("Jwt:ExpiryMinutes") ?? 1440;
            var expires = DateTime.UtcNow.AddMinutes(minutes);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var jwt = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return (new JwtSecurityTokenHandler().WriteToken(jwt), expires);
        }
    }
}
