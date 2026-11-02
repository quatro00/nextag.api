using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Dto.Auth;
using Nextag.Api.Repositories.Interface;

namespace Nextag.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly ITokenRepository tokenRepository;
        private readonly NextagContext context;

        public AuthController(
            UserManager<IdentityUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ITokenRepository tokenRepository,
            NextagContext context
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

            var sistemaId = await context.AspNetSystems.AsNoTracking()
                .Where(x => x.Clave == "NEXTAG")
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();

            if (!sistemaId.HasValue)
                return Unauthorized(new { message = "El sistema NEXTAG no está configurado." });

            var rolesDisponibles = await roleManager.Roles.AsNoTracking()
                .Where(x => x.SistemaId == sistemaId.Value && x.Name != null)
                .Select(x => x.Name!)
                .ToListAsync();

            var rolesUsuario = await userManager.GetRolesAsync(user);

            var roles = rolesUsuario
                .Where(x => rolesDisponibles.Contains(x, StringComparer.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (roles.Count != 1)
                return Unauthorized(new { message = "El usuario debe tener exactamente un rol de NexTag." });

            if (!roles[0].Equals("ADMINISTRADOR", StringComparison.OrdinalIgnoreCase))
            {
                var tieneEmpresa = await context.UsuarioEmpresas.AsNoTracking()
                    .AnyAsync(x => x.UsuarioId == usuario.Id && x.Activo == true && x.Empresa.Activo == true);

                if (!tieneEmpresa)
                    return Unauthorized(new { message = "El usuario no tiene empresas activas asignadas." });
            }

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
