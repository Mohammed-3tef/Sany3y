using Microsoft.IdentityModel.Tokens;
using Sany3y.Infrastructure.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static System.Net.WebRequestMethods;

namespace Sany3y.Services
{
    public class JwtTokenService
    {
        private readonly IConfiguration _config;
        private HttpClient _http;

        public JwtTokenService(IConfiguration config, HttpClient http)
        {
            _config = config;
            _http = http;
        }

        public async Task<string> GenerateTokenAsync(User user)
        {
            var response = await _http.GetAsync($"/api/Role/GetAll");
            var allRoles = await response.Content.ReadFromJsonAsync<IList<Role>>();
            var roles = allRoles?.Select(r => r.Name).ToList();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, roles?.FirstOrDefault() ?? "User")
            };

            var key = new SymmetricSecurityKey(
                Convert.FromBase64String(_config["Jwt:Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:ExpiresInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
