using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.ActivoMovimientos;
using Nextag.Api.Services.ActivoMovimientos.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/activo-movimientos")]
    [Authorize(
    Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
    [RequiereEmpresa]
    public class ActivoMovimientosController : ControllerBase
    {
        private readonly IActivoMovimientoService service;

        public ActivoMovimientosController(
            IActivoMovimientoService service)
        {
            this.service = service;
        }

        // ==========================================
        // LISTADO
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] ActivoMovimientoFiltroDto filtros,
            CancellationToken ct)
        {
            var resultado = await service.GetAllAsync(
                HttpContext.GetEmpresaId(),
                filtros,
                ct);

            return Ok(resultado);
        }

        // ==========================================
        // CATALOGO
        // ==========================================

        [HttpGet("tipos")]
        public async Task<IActionResult> GetTipos(
            CancellationToken ct)
        {
            var resultado = await service.GetTiposAsync(ct);

            return Ok(resultado);
        }

        // ==========================================
        // HISTORIAL DE ACTIVO
        // ==========================================

        [HttpGet("activo/{activoId:guid}/historial")]
        public async Task<IActionResult> GetHistorialActivo(
            Guid activoId,
            CancellationToken ct)
        {
            var resultado = await service.GetHistorialActivoAsync(
                activoId,
                HttpContext.GetEmpresaId(),
                ct);

            return Ok(resultado);
        }

        // ==========================================
        // SITUACION FISICA DEL ACTIVO
        // ==========================================

        [HttpGet("activo/{activoId:guid}/situacion")]
        public async Task<IActionResult> GetSituacionActivo(
            Guid activoId,
            CancellationToken ct)
        {
            var resultado = await service.GetSituacionAsync(
                activoId,
                HttpContext.GetEmpresaId(),
                ct);

            if (resultado == null)
            {
                return NotFound(new
                {
                    message = "Activo no encontrado."
                });
            }

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
            var resultado = await service.GetByIdAsync(
                id,
                HttpContext.GetEmpresaId(),
                ct);

            if (resultado == null)
            {
                return NotFound(new
                {
                    message = "Movimiento no encontrado."
                });
            }

            return Ok(resultado);
        }

        // ==========================================
        // REGISTRAR MOVIMIENTO
        // ==========================================

        [HttpPost]
        [Authorize(
            Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Create(
            [FromBody] ActivoMovimientoCrearDto dto,
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
    }
}
