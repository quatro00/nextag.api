using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.Tickets;
using Nextag.Api.Services.Tickets.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
    [RequiereEmpresa]
    public class TicketEstadosController : ControllerBase
    {
        private readonly ITicketEstadoService service;
        private readonly ITicketService ticketService;

        public TicketEstadosController(
            ITicketEstadoService service,
            ITicketService ticketService)
        {
            this.service = service;
            this.ticketService = ticketService;
        }

        // ==========================================
        // HISTORIAL
        // ==========================================

        [HttpGet("{id:guid}/historial-estados")]
        public async Task<IActionResult> GetHistorial(
            Guid id,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();

            var ticket = await ticketService.GetByIdAsync(
                id,
                empresaId,
                ct);

            if (ticket == null)
            {
                return NotFound(new
                {
                    message = "Ticket no encontrado."
                });
            }

            var historial = await service.GetHistorialAsync(
                id,
                empresaId,
                ct);

            return Ok(historial);
        }

        // ==========================================
        // INICIAR
        // ==========================================

        [HttpPatch("{id:guid}/iniciar")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR")]
        public async Task<IActionResult> Iniciar(
            Guid id,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();
            var usuarioId = User.GetUsuarioId();

            var esAdministrador =
                User.IsInRole("ADMINISTRADOR") ||
                User.IsInRole("ADMIN_EMPRESA");

            var result = await service.IniciarAsync(
                id,
                empresaId,
                usuarioId,
                esAdministrador,
                ct);

            return ProcesarResultado(result);
        }

        // ==========================================
        // RESOLVER
        // ==========================================

        [HttpPatch("{id:guid}/resolver")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR")]
        public async Task<IActionResult> Resolver(
            Guid id,
            [FromBody] TicketResolverDto dto,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();
            var usuarioId = User.GetUsuarioId();

            var esAdministrador =
                User.IsInRole("ADMINISTRADOR") ||
                User.IsInRole("ADMIN_EMPRESA");

            var result = await service.ResolverAsync(
                id,
                empresaId,
                usuarioId,
                esAdministrador,
                dto,
                ct);

            return ProcesarResultado(result);
        }

        // ==========================================
        // CERRAR
        // ==========================================

        [HttpPatch("{id:guid}/cerrar")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Cerrar(
            Guid id,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();
            var usuarioId = User.GetUsuarioId();

            var result = await service.CerrarAsync(
                id,
                empresaId,
                usuarioId,
                ct);

            return ProcesarResultado(result);
        }

        // ==========================================
        // REABRIR
        // ==========================================

        [HttpPatch("{id:guid}/reabrir")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Reabrir(
            Guid id,
            [FromBody] TicketReabrirDto dto,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();
            var usuarioId = User.GetUsuarioId();

            var result = await service.ReabrirAsync(
                id,
                empresaId,
                usuarioId,
                dto,
                ct);

            return ProcesarResultado(result);
        }

        // ==========================================
        // CANCELAR
        // ==========================================

        [HttpPatch("{id:guid}/cancelar")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Cancelar(
            Guid id,
            [FromBody] TicketCancelarDto dto,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();
            var usuarioId = User.GetUsuarioId();

            var result = await service.CancelarAsync(
                id,
                empresaId,
                usuarioId,
                dto,
                ct);

            return ProcesarResultado(result);
        }

        // ==========================================
        // RESULTADOS
        // ==========================================

        private IActionResult ProcesarResultado(
            Nextag.Api.Models.Dto.ServiceResult<TicketDetalleDto> result)
        {
            if (!result.Success)
            {
                if (result.Message == "Ticket no encontrado.")
                {
                    return NotFound(new
                    {
                        message = result.Message
                    });
                }

                return BadRequest(new
                {
                    message = result.Message
                });
            }

            return Ok(result.Data);
        }
    }
}
