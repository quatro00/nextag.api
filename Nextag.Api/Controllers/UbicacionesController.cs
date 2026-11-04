using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.Ubicaciones;
using Nextag.Api.Services.Ubicaciones.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/ubicaciones")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
    [RequiereEmpresa]
    public class UbicacionesController : ControllerBase
    {
        private readonly IUbicacionService service;

        public UbicacionesController(IUbicacionService service)
        {
            this.service = service;
        }

        // ==========================================
        // GET ALL
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? sucursalId,
            [FromQuery] bool? activo,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();

            var ubicaciones = await service.GetAllAsync(
                empresaId,
                sucursalId,
                activo,
                ct);

            return Ok(ubicaciones);
        }

        // ==========================================
        // GET BY ID
        // ==========================================

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();

            var ubicacion = await service.GetByIdAsync(
                id,
                empresaId,
                ct);

            if (ubicacion == null)
            {
                return NotFound(new
                {
                    message = "Ubicación no encontrada."
                });
            }

            return Ok(ubicacion);
        }

        // ==========================================
        // CREATE
        // ==========================================

        [HttpPost]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Create(
            [FromBody] UbicacionCreateDto dto,
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
        // UPDATE
        // ==========================================

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UbicacionUpdateDto dto,
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
                if (result.Message == "Ubicación no encontrada.")
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
        // UPDATE STATUS
        // ==========================================

        [HttpPatch("{id:guid}/estatus")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UbicacionEstatusDto dto,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();
            var usuarioId = User.GetUsuarioId();

            var result = await service.UpdateStatusAsync(
                id,
                empresaId,
                dto.Activo,
                usuarioId,
                ct);

            if (!result.Success)
            {
                if (result.Message == "Ubicación no encontrada.")
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
                message = dto.Activo
                    ? "Ubicación activada correctamente."
                    : "Ubicación desactivada correctamente."
            });
        }

        // ==========================================
        // DELETE LOGICO
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
                if (result.Message == "Ubicación no encontrada.")
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
