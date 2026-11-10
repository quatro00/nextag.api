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
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService service;

        public TicketsController(ITicketService service)
        {
            this.service = service;
        }

        // ==========================================
        // LISTADO
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] TicketFiltroDto filtro,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();

            if (filtro.FechaInicio.HasValue &&
                filtro.FechaFin.HasValue &&
                filtro.FechaInicio.Value.Date >
                filtro.FechaFin.Value.Date)
            {
                return BadRequest(new
                {
                    message = "La fecha inicial no puede ser mayor que la fecha final."
                });
            }

            var resultado = await service.GetAllAsync(
                empresaId,
                filtro,
                ct);

            return Ok(resultado);
        }

        // ==========================================
        // DETALLE
        // ==========================================

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();

            var ticket = await service.GetByIdAsync(
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

            return Ok(ticket);
        }

        // ==========================================
        // TICKETS POR ACTIVO
        // ==========================================

        [HttpGet("activo/{activoId:guid}")]
        public async Task<IActionResult> GetByActivo(
            Guid activoId,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();

            var tickets = await service.GetByActivoAsync(
                activoId,
                empresaId,
                ct);

            return Ok(tickets);
        }

        // ==========================================
        // ESTADOS
        // ==========================================

        [HttpGet("estados")]
        public async Task<IActionResult> GetEstados(
            CancellationToken ct)
        {
            var estados = await service.GetEstadosAsync(ct);

            return Ok(estados);
        }

        // ==========================================
        // PRIORIDADES
        // ==========================================

        [HttpGet("prioridades")]
        public async Task<IActionResult> GetPrioridades(
            CancellationToken ct)
        {
            var prioridades = await service.GetPrioridadesAsync(ct);

            return Ok(prioridades);
        }

        // ==========================================
        // CREAR
        // ==========================================

        [HttpPost]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR")]
        public async Task<IActionResult> Create(
            [FromBody] TicketCrearDto dto,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();
            var usuarioId = User.GetUsuarioId();

            var result = await service.CreateAsync(
                dto,
                empresaId,
                usuarioId,
                ct);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    message = result.Message
                });
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data!.Id },
                result.Data);
        }

        // ==========================================
        // ACTUALIZAR
        // ==========================================

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] TicketActualizarDto dto,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();
            var usuarioId = User.GetUsuarioId();

            var result = await service.UpdateAsync(
                id,
                dto,
                empresaId,
                usuarioId,
                ct);

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
