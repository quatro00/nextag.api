using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Services.Notificaciones.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/notificaciones")]
    [Authorize(Roles = "ADMINISTRADOR")]
    public class NotificacionProcesadorController
         : ControllerBase
    {
        private readonly INotificacionProcesadorService service;

        public NotificacionProcesadorController(
            INotificacionProcesadorService service)
        {
            this.service = service;
        }

        // ==========================================
        // PROCESAR EVENTOS PENDIENTES
        // ==========================================

        [HttpPost("procesar-pendientes")]
        public async Task<IActionResult> ProcesarPendientes(
            [FromQuery] int cantidad = 20,
            CancellationToken ct = default)
        {
            var resultado =
                await service.ProcesarPendientesAsync(
                    cantidad,
                    ct);

            return Ok(resultado);
        }
    }
}
