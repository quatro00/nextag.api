using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.EquipoSoporteIntegrantes;
using Nextag.Api.Services.EquipoSoporteIntegrantes.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/equipos-soporte/{equipoId:guid}/integrantes")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
    [RequiereEmpresa]
    public class EquipoSoporteIntegrantesController : ControllerBase
    {
        private readonly IEquipoSoporteIntegranteService service;

        public EquipoSoporteIntegrantesController(
            IEquipoSoporteIntegranteService service)
        {
            this.service = service;
        }

        // ==========================================
        // LISTADO
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetAll(
            Guid equipoId,
            [FromQuery] bool? activo,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();

            var integrantes = await service.GetAllAsync(
                empresaId,
                equipoId,
                activo,
                ct);

            return Ok(integrantes);
        }

        // ==========================================
        // USUARIOS DISPONIBLES
        // ==========================================

        [HttpGet("disponibles")]
        public async Task<IActionResult> GetDisponibles(
            Guid equipoId,
            [FromQuery] string? buscar,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();

            var usuarios = await service.GetDisponiblesAsync(
                empresaId,
                equipoId,
                buscar,
                ct);

            return Ok(usuarios);
        }

        // ==========================================
        // DETALLE
        // ==========================================

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid equipoId,
            Guid id,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();

            var integrante = await service.GetByIdAsync(
                empresaId,
                equipoId,
                id,
                ct);

            if (integrante == null)
            {
                return NotFound(new
                {
                    message = "Integrante no encontrado."
                });
            }

            return Ok(integrante);
        }

        // ==========================================
        // AGREGAR INTEGRANTE
        // ==========================================

        [HttpPost]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Create(
            Guid equipoId,
            [FromBody] EquipoSoporteIntegranteCrearDto dto,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();
            var usuarioId = User.GetUsuarioId();

            var result = await service.CreateAsync(
                empresaId,
                equipoId,
                dto,
                usuarioId,
                ct);

            if (!result.Success)
            {
                if (result.Message == "Equipo de soporte no encontrado.")
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

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    equipoId,
                    id = result.Data!.Id
                },
                result.Data);
        }

        // ==========================================
        // ACTIVAR / DESACTIVAR
        // ==========================================

        [HttpPatch("{id:guid}/estatus")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> UpdateStatus(
            Guid equipoId,
            Guid id,
            [FromBody] EquipoSoporteIntegranteEstatusDto dto,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();
            var usuarioId = User.GetUsuarioId();

            var result = await service.UpdateStatusAsync(
                empresaId,
                equipoId,
                id,
                dto.Activo!.Value,
                usuarioId,
                ct);

            if (!result.Success)
            {
                if (result.Message == "Integrante no encontrado.")
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

            return Ok(new
            {
                message = dto.Activo.Value
                    ? "Integrante activado correctamente."
                    : "Integrante desactivado correctamente."
            });
        }

        // ==========================================
        // ELIMINACION LOGICA
        // ==========================================

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Delete(
            Guid equipoId,
            Guid id,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();
            var usuarioId = User.GetUsuarioId();

            var result = await service.UpdateStatusAsync(
                empresaId,
                equipoId,
                id,
                false,
                usuarioId,
                ct);

            if (!result.Success)
            {
                if (result.Message == "Integrante no encontrado.")
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

            return NoContent();
        }
    }
}
