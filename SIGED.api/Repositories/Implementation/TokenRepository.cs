using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SIGED.api.Repositories.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SIGED.api.Repositories.Implementation
{
    public class TokenRepository : ITokenRepository
    {
        private readonly IConfiguration configuration;

        public TokenRepository(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string CreateJwtToken(IdentityUser user, string usuarioId, List<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("Id", user.Id),
                new Claim("UsuarioId", usuarioId)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var jwtKey = configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException("No se encontró la configuración Jwt:Key.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(600),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
