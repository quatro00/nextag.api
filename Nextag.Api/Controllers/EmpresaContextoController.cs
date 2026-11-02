using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Helpers;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/contexto")]
    [Authorize]
    public class EmpresaContextoController : ControllerBase
    {
        private readonly NextagContext context;

        public EmpresaContextoController(NextagContext context)
        {
            this.context = context;
        }

        [HttpGet("empresas")]
        public async Task<IActionResult> GetEmpresas()
        {
            var usuarioId = User.GetUsuarioId();

            var usuarioActivo = await context.Usuarios.AsNoTracking()
                .AnyAsync(x => x.Id == usuarioId && x.Activo == true);

            if (!usuarioActivo) return Forbid();

            var empresas = User.IsInRole("ADMINISTRADOR")
                ? context.Empresas.Where(x => x.Activo == true)
                : context.UsuarioEmpresas
                    .Where(x => x.UsuarioId == usuarioId && x.Activo == true)
                    .Select(x => x.Empresa)
                    .Where(x => x.Activo == true);

            var resultado = await empresas.AsNoTracking()
                .OrderBy(x => x.Nombre)
                .Select(x => new { x.Id, x.Clave, x.Nombre })
                .ToListAsync();

            return Ok(resultado);
        }
    }
}
