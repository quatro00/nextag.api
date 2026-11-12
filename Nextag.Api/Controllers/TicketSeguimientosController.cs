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
    [Route("api/tickets/{ticketId:guid}")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
    [RequiereEmpresa]
    public class TicketSeguimientosController : ControllerBase
    {
        private readonly ITicketSeguimientoService service;

        public TicketSeguimientosController(
            ITicketSeguimientoService service)
        {
            this.service = service;
        }

        // ==========================================
        // LISTADO
        // ==========================================

        [HttpGet("seguimientos")]
        public async Task<IActionResult> GetAll(
            Guid ticketId,
            CancellationToken ct)
        {
            var resultado = await service.GetAllAsync(
                ticketId,
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
                EsAdministrador(),
                ct);

            return ProcesarLista(resultado);
        }

        // ==========================================
        // DETALLE
        // ==========================================

        [HttpGet("seguimientos/{seguimientoId:guid}")]
        public async Task<IActionResult> GetById(
            Guid ticketId,
            Guid seguimientoId,
            CancellationToken ct)
        {
            var resultado = await service.GetByIdAsync(
                ticketId,
                seguimientoId,
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
                EsAdministrador(),
                ct);

            return ProcesarResultado(resultado);
        }

        // ==========================================
        // COMENTARIO
        // ==========================================

        [HttpPost("comentarios")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR")]
        public async Task<IActionResult> AgregarComentario(
            Guid ticketId,
            [FromBody] TicketSeguimientoCrearDto dto,
            CancellationToken ct)
        {
            var resultado = await service.AgregarComentarioAsync(
                ticketId,
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
                EsAdministrador(),
                dto,
                ct);

            if (!resultado.Success)
            {
                return ProcesarResultado(resultado);
            }

            return StatusCode(
                StatusCodes.Status201Created,
                resultado.Data);
        }

        // ==========================================
        // NOTA INTERNA
        // ==========================================

        [HttpPost("notas-internas")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR")]
        public async Task<IActionResult> AgregarNotaInterna(
            Guid ticketId,
            [FromBody] TicketSeguimientoCrearDto dto,
            CancellationToken ct)
        {
            var resultado = await service.AgregarNotaInternaAsync(
                ticketId,
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
                EsAdministrador(),
                dto,
                ct);

            if (!resultado.Success)
            {
                return ProcesarResultado(resultado);
            }

            return StatusCode(
                StatusCodes.Status201Created,
                resultado.Data);
        }

        // ==========================================
        // ADMINISTRADOR
        // ==========================================

        private bool EsAdministrador()
        {
            return User.IsInRole("ADMINISTRADOR") ||
                   User.IsInRole("ADMIN_EMPRESA");
        }

        // ==========================================
        // RESPUESTA INDIVIDUAL
        // ==========================================

        private IActionResult ProcesarResultado(
            ServiceResult<TicketSeguimientoDto> resultado)
        {
            if (resultado.Success)
            {
                return Ok(resultado.Data);
            }

            if (resultado.Message is
                "Ticket no encontrado." or
                "Seguimiento no encontrado.")
            {
                return NotFound(new
                {
                    message = resultado.Message
                });
            }

            if (resultado.Message != null &&
                resultado.Message.StartsWith(
                    "No tienes permiso",
                    StringComparison.Ordinal))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = resultado.Message });
            }

            return BadRequest(new
            {
                message = resultado.Message
            });
        }

        // ==========================================
        // RESPUESTA DE LISTADO
        // ==========================================

        private IActionResult ProcesarLista(
            ServiceResult<List<TicketSeguimientoDto>> resultado)
        {
            if (resultado.Success)
            {
                return Ok(resultado.Data);
            }

            if (resultado.Message == "Ticket no encontrado.")
            {
                return NotFound(new
                {
                    message = resultado.Message
                });
            }

            if (resultado.Message != null &&
                resultado.Message.StartsWith(
                    "No tienes permiso",
                    StringComparison.Ordinal))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = resultado.Message });
            }

            return BadRequest(new
            {
                message = resultado.Message
            });
        }
    }
}
