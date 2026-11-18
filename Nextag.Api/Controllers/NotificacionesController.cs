using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.Notificaciones;
using Nextag.Api.Services.Notificaciones.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/notificaciones")]
    [Authorize(
        Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
    [RequiereEmpresa]
    public class NotificacionesController : ControllerBase
    {
        private readonly INotificacionInternaService service;

        public NotificacionesController(
            INotificacionInternaService service)
        {
            this.service = service;
        }

        // ==========================================
        // LISTAR NOTIFICACIONES
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] NotificacionConsultaDto consulta,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();

            var usuarioId = User.GetUsuarioId();

            var resultado = await service.ObtenerAsync(
                empresaId,
                usuarioId,
                consulta,
                ct);

            return Ok(resultado);
        }

        // ==========================================
        // CONTADOR NO LEIDAS
        // ==========================================

        [HttpGet("no-leidas")]
        public async Task<IActionResult> ContarNoLeidas(
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();

            var usuarioId = User.GetUsuarioId();

            var total = await service.ContarNoLeidasAsync(
                empresaId,
                usuarioId,
                ct);

            return Ok(new NotificacionContadorDto
            {
                NoLeidas = total
            });
        }

        // ==========================================
        // MARCAR UNA COMO LEIDA
        // ==========================================

        [HttpPut("{id:guid}/leer")]
        public async Task<IActionResult> MarcarLeida(
            Guid id,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();

            var usuarioId = User.GetUsuarioId();

            var resultado = await service.MarcarLeidaAsync(
                empresaId,
                usuarioId,
                id,
                ct);

            if (!resultado)
            {
                return NotFound(new
                {
                    message = "Notificación no encontrada."
                });
            }

            return Ok(new
            {
                message = "Notificación marcada como leída."
            });
        }

        // ==========================================
        // MARCAR TODAS COMO LEIDAS
        // ==========================================

        [HttpPut("leer-todas")]
        public async Task<IActionResult> MarcarTodasLeidas(
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();

            var usuarioId = User.GetUsuarioId();

            var actualizadas =
                await service.MarcarTodasLeidasAsync(
                    empresaId,
                    usuarioId,
                    ct);

            return Ok(new
            {
                message = "Notificaciones actualizadas.",
                actualizadas
            });
        }
    }
}
