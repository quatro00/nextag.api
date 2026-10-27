using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGED.api.Data;
using SIGED.api.Models.Dto.Auth;
using SIGED.api.Repositories.Interface;

namespace SIGED.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly ITokenRepository tokenRepository;
        private readonly SigedContext context;

        public AuthController(
            UserManager<IdentityUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ITokenRepository tokenRepository,
            SigedContext context
            )
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.tokenRepository = tokenRepository;
            this.context = context;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var emailNormalizado = request.Email.Trim().ToLower();

            var user = await userManager.FindByEmailAsync(emailNormalizado);

            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "Usuario o contraseña incorrectos."
                });
            }

            var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);

            if (!passwordValid)
            {
                return Unauthorized(new
                {
                    message = "Usuario o contraseña incorrectos."
                });
            }

            var usuario = await context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.AspNetUserId == user.Id);

            if (usuario == null)
            {
                return Unauthorized(new
                {
                    message = "El usuario no tiene perfil configurado."
                });
            }

            if (usuario.Activo != true)
            {
                return Unauthorized(new
                {
                    message = "El usuario se encuentra inactivo."
                });
            }

            var roles = (await userManager.GetRolesAsync(user)).ToList();

            var token = tokenRepository.CreateJwtToken(user, usuario.Id.ToString(), roles);

            var response = new LoginResponseDto()
            {
                AccessToken = token,
                TokenType = "bearer",
                User = new UserDto()
                {
                    Id = user.Id,
                    Name = $"{usuario.Nombre} {usuario.Apellidos}".Trim(),
                    Avatar = "",
                    Roles = roles.ToList(),
                    Status = "online",
                    Email = user.Email,
                }
            };

            await ActualizarUltimoAccesoAsync(usuario.Id);

            return Ok(response);
        }

        private async Task ActualizarUltimoAccesoAsync(Guid usuarioId)
        {
            var usuario = await context.Usuarios.FirstOrDefaultAsync(x => x.Id == usuarioId);

            if (usuario == null)
            {
                return;
            }

            usuario.FechaUltimoAcceso = DateTime.UtcNow;
            usuario.FechaActualizacion = DateTime.UtcNow;

            await context.SaveChangesAsync();
        }

        private static List<string> NormalizarRoles(List<string>? roles)
        {
            if (roles == null)
            {
                return new List<string>();
            }

            return roles
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToUpper())
                .Distinct()
                .ToList();
        }

        private static string? NormalizarNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string? NormalizarDominio(string? dominio)
        {
            if (string.IsNullOrWhiteSpace(dominio))
            {
                return null;
            }

            var value = dominio.Trim().ToLower();

            value = value
                .Replace("https://", "")
                .Replace("http://", "")
                .Replace("www.", "");

            return value.Trim('/');
        }
    }
}
