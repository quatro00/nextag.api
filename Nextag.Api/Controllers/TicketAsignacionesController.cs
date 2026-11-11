using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.Tickets;
using Nextag.Api.Models.Dto;
using Nextag.Api.Services.Tickets.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
    [RequiereEmpresa]
    public class TicketAsignacionesController : ControllerBase
    {
        private readonly ITicketAsignacionService service;
        private readonly ITicketService ticketService;

        public TicketAsignacionesController(
            ITicketAsignacionService service,
            ITicketService ticketService)
        {
            this.service = service;
            this.ticketService = ticketService;
        }

        // ==========================================
        // PRIMERA ASIGNACION
        // ==========================================

        [HttpPost("{id:guid}/asignaciones")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Asignar(
            Guid id,
            [FromBody] TicketAsignarDto dto,
            CancellationToken ct)
        {
            var result = await service.AsignarAsync(
                id,
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
                dto,
                ct);

            return ProcesarResultado(result);
        }

        // ==========================================
        // REASIGNAR
        // ==========================================

        [HttpPost("{id:guid}/reasignaciones")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Reasignar(
            Guid id,
            [FromBody] TicketReasignarDto dto,
            CancellationToken ct)
        {
            var result = await service.ReasignarAsync(
                id,
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
                dto,
                ct);

            return ProcesarResultado(result);
        }

        // ==========================================
        // ASIGNACION VIGENTE
        // ==========================================

        [HttpGet("{id:guid}/asignacion-vigente")]
        public async Task<IActionResult> GetVigente(
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

            var asignacion = await service.GetVigenteAsync(
                id,
                empresaId,
                ct);

            return Ok(asignacion);
        }

        // ==========================================
        // HISTORIAL
        // ==========================================

        [HttpGet("{id:guid}/historial-asignaciones")]
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
        // MIS TICKETS
        // ==========================================

        [HttpGet("mis-tickets")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR")]
        public async Task<IActionResult> GetMisTickets(
            CancellationToken ct)
        {
            var resultado = await service.GetMisTicketsAsync(
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
                ct);

            return Ok(resultado);
        }

        // ==========================================
        // TICKETS DE MIS EQUIPOS
        // ==========================================

        [HttpGet("mis-equipos")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR")]
        public async Task<IActionResult> GetTicketsMisEquipos(
            CancellationToken ct)
        {
            var resultado = await service.GetTicketsMisEquiposAsync(
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
                ct);

            return Ok(resultado);
        }

        // ==========================================
        // PROCESAR RESULTADO
        // ==========================================

        private IActionResult ProcesarResultado(
            ServiceResult<TicketAsignacionDto> result)
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
