using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.ActivoAsignaciones;
using Nextag.Api.Services.ActivoAsignaciones.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/activo-asignaciones")]
    [Authorize(
    Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
    [RequiereEmpresa]
    public class ActivoAsignacionesController : ControllerBase
    {
        private readonly IActivoAsignacionService service;

        public ActivoAsignacionesController(
            IActivoAsignacionService service)
        {
            this.service = service;
        }

        // ==================================================
        // LISTADO
        // ==================================================

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] ActivoAsignacionFiltroDto filtros,
            CancellationToken ct)
        {
            var resultado = await service.GetAllAsync(
                HttpContext.GetEmpresaId(),
                filtros,
                ct);

            return Ok(resultado);
        }

        // ==================================================
        // ACTIVOS SIN ASIGNAR
        // ==================================================

        [HttpGet("activos-sin-asignar")]
        public async Task<IActionResult> GetActivosSinAsignar(
            [FromQuery] string? buscar,
            [FromQuery] Guid? sucursalId,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 20,
            CancellationToken ct = default)
        {
            var resultado =
                await service.GetActivosSinAsignarAsync(
                    HttpContext.GetEmpresaId(),
                    buscar,
                    sucursalId,
                    pagina,
                    tamanoPagina,
                    ct);

            return Ok(resultado);
        }

        // ==================================================
        // HISTORIAL DE ACTIVO
        // ==================================================

        [HttpGet("activo/{activoId:guid}/historial")]
        public async Task<IActionResult> GetHistorialActivo(
            Guid activoId,
            CancellationToken ct)
        {
            var resultado =
                await service.GetHistorialActivoAsync(
                    activoId,
                    HttpContext.GetEmpresaId(),
                    ct);

            return Ok(resultado);
        }

        // ==================================================
        // ASIGNACION VIGENTE DE ACTIVO
        // ==================================================

        [HttpGet("activo/{activoId:guid}/vigente")]
        public async Task<IActionResult> GetVigenteActivo(
            Guid activoId,
            CancellationToken ct)
        {
            var resultado =
                await service.GetVigenteActivoAsync(
                    activoId,
                    HttpContext.GetEmpresaId(),
                    ct);

            if (resultado == null)
            {
                return NotFound(new
                {
                    message =
                        "El activo no tiene una asignación vigente."
                });
            }

            return Ok(resultado);
        }

        // ==================================================
        // HISTORIAL DE RESPONSABLE
        // ==================================================

        [HttpGet("responsable/{responsableId:guid}/historial")]
        public async Task<IActionResult> GetHistorialResponsable(
            Guid responsableId,
            CancellationToken ct)
        {
            var resultado =
                await service.GetHistorialResponsableAsync(
                    responsableId,
                    HttpContext.GetEmpresaId(),
                    ct);

            return Ok(resultado);
        }

        // ==================================================
        // DETALLE
        // ==================================================

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken ct)
        {
            var resultado = await service.GetByIdAsync(
                id,
                HttpContext.GetEmpresaId(),
                ct);

            if (resultado == null)
            {
                return NotFound(new
                {
                    message = "Asignación no encontrada."
                });
            }

            return Ok(resultado);
        }

        // ==================================================
        // CREAR ASIGNACION
        // ==================================================

        [HttpPost]
        [Authorize(
            Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Create(
            [FromBody] ActivoAsignacionCrearDto dto,
            CancellationToken ct)
        {
            var resultado = await service.CreateAsync(
                dto,
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
                ct);

            if (!resultado.Success)
            {
                return BadRequest(new
                {
                    message = resultado.Message
                });
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = resultado.Data!.Id },
                resultado.Data);
        }

        // ==================================================
        // DEVOLVER
        // ==================================================

        [HttpPost("{id:guid}/devolver")]
        [Authorize(
            Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Devolver(
            Guid id,
            [FromBody] ActivoAsignacionDevolverDto dto,
            CancellationToken ct)
        {
            var resultado = await service.DevolverAsync(
                id,
                dto,
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
                ct);

            if (!resultado.Success)
            {
                if (resultado.Message ==
                    "Asignación no encontrada.")
                {
                    return NotFound(new
                    {
                        message = resultado.Message
                    });
                }

                return BadRequest(new
                {
                    message = resultado.Message
                });
            }

            return Ok(resultado.Data);
        }

        // ==================================================
        // REASIGNAR
        // ==================================================

        [HttpPost("{id:guid}/reasignar")]
        [Authorize(
            Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Reasignar(
            Guid id,
            [FromBody] ActivoAsignacionReasignarDto dto,
            CancellationToken ct)
        {
            var resultado = await service.ReasignarAsync(
                id,
                dto,
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
                ct);

            if (!resultado.Success)
            {
                if (resultado.Message ==
                    "Asignación no encontrada.")
                {
                    return NotFound(new
                    {
                        message = resultado.Message
                    });
                }

                return BadRequest(new
                {
                    message = resultado.Message
                });
            }

            return Ok(resultado.Data);
        }
    }
}
