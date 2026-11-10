using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.EquiposSoporte;
using Nextag.Api.Services.EquiposSoporte.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/equipos-soporte")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
    [RequiereEmpresa]
    public class EquiposSoporteController : ControllerBase
    {
        private readonly IEquipoSoporteService service;

        public EquiposSoporteController(
            IEquipoSoporteService service)
        {
            this.service = service;
        }

        // ==========================================
        // LISTADO
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] bool? activo,
            [FromQuery] string? tipo,
            [FromQuery] string? buscar,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();

            var equipos = await service.GetAllAsync(
                empresaId,
                activo,
                tipo,
                buscar,
                ct);

            return Ok(equipos);
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

            var equipo = await service.GetByIdAsync(
                id,
                empresaId,
                ct);

            if (equipo == null)
            {
                return NotFound(new
                {
                    message = "Equipo de soporte no encontrado."
                });
            }

            return Ok(equipo);
        }

        // ==========================================
        // CREAR
        // ==========================================

        [HttpPost]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Create(
            [FromBody] EquipoSoporteGuardarDto dto,
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
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] EquipoSoporteGuardarDto dto,
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

            return Ok(result.Data);
        }

        // ==========================================
        // ACTIVAR / DESACTIVAR
        // ==========================================

        [HttpPatch("{id:guid}/estatus")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] EquipoSoporteEstatusDto dto,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();
            var usuarioId = User.GetUsuarioId();

            var result = await service.UpdateStatusAsync(
                id,
                empresaId,
                dto.Activo!.Value,
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

            return Ok(new
            {
                message = dto.Activo.Value
                    ? "Equipo activado correctamente."
                    : "Equipo desactivado correctamente."
            });
        }

        // ==========================================
        // ELIMINACION LOGICA
        // ==========================================

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();
            var usuarioId = User.GetUsuarioId();

            var result = await service.UpdateStatusAsync(
                id,
                empresaId,
                false,
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

            return NoContent();
        }
    }
}
