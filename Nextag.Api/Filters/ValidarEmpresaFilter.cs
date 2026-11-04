using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Nextag.Api.Filters
{
    public class ValidarEmpresaFilter : IAsyncActionFilter
    {
        private readonly NextagContext context;

        public ValidarEmpresaFilter(NextagContext context)
        {
            this.context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
        {
            var http = ctx.HttpContext;
            var ct = http.RequestAborted;

            if (http.User.Identity?.IsAuthenticated != true)
            {
                ctx.Result = new UnauthorizedObjectResult(new { message = "Usuario no autenticado." });
                return;
            }

            var empresaHeader = http.Request.Headers["X-Empresa-Id"].FirstOrDefault();

            if (!Guid.TryParse(empresaHeader, out var empresaId) || empresaId == Guid.Empty)
            {
                ctx.Result = new BadRequestObjectResult(new { message = "Debes seleccionar una empresa válida." });
                return;
            }

            if (!Guid.TryParse(http.User.FindFirst("UsuarioId")?.Value, out var usuarioId))
            {
                ctx.Result = new UnauthorizedObjectResult(new { message = "Identidad de usuario inválida." });
                return;
            }

            var usuario = await context.Usuarios.AsNoTracking()
                .Where(x => x.Id == usuarioId && x.Activo == true)
                .Select(x => new { x.AspNetUserId })
                .FirstOrDefaultAsync(ct);

            if (usuario == null)
            {
                ctx.Result = new ObjectResult(new { message = "El usuario no está activo." }) { StatusCode = 403 };
                return;
            }

            var rolesActuales = await context.AspNetUsers.AsNoTracking()
                .Where(x => x.Id == usuario.AspNetUserId)
                .SelectMany(x => x.Roles)
                .Where(x => x.Sistema != null && x.Sistema.Clave == "NEXTAG" && x.Name != null)
                .Select(x => x.Name!)
                .ToListAsync(ct);

            if (rolesActuales.Count != 1 || !http.User.IsInRole(rolesActuales[0]))
            {
                ctx.Result = new ObjectResult(new { message = "Los permisos del usuario han cambiado. Inicia sesión nuevamente." }) { StatusCode = 403 };
                return;
            }

            var empresaActiva = await context.Empresas.AsNoTracking()
                .AnyAsync(x => x.Id == empresaId && x.Activo == true, ct);

            if (!empresaActiva)
            {
                ctx.Result = new ObjectResult(new { message = "La empresa seleccionada no está disponible." }) { StatusCode = 403 };
                return;
            }

            var esAdministradorGlobal = rolesActuales[0].Equals("ADMINISTRADOR", StringComparison.OrdinalIgnoreCase);

            if (!esAdministradorGlobal)
            {
                var tieneAcceso = await context.UsuarioEmpresas.AsNoTracking()
                    .AnyAsync(x => x.UsuarioId == usuarioId && x.EmpresaId == empresaId && x.Activo == true, ct);

                if (!tieneAcceso)
                {
                    ctx.Result = new ObjectResult(new { message = "No tienes acceso a la empresa seleccionada." }) { StatusCode = 403 };
                    return;
                }
            }

            http.Items["EmpresaId"] = empresaId;
            await next();
        }
    }
}
