using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.TiposUbicacion;
using Nextag.Api.Services.TiposUbicacion.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/tipos-ubicacion")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
    [RequiereEmpresa]
    public class TiposUbicacionController : ControllerBase
    {
        private readonly ITipoUbicacionService service;

        public TiposUbicacionController(ITipoUbicacionService service)
        {
            this.service = service;
        }

        // ==========================================
        // GET ALL
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] bool? activo,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();

            var tipos = await service.GetAllAsync(
                empresaId,
                activo,
                ct);

            return Ok(tipos);
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

            var tipo = await service.GetByIdAsync(
                id,
                empresaId,
                ct);

            if (tipo == null)
            {
                return NotFound(new
                {
                    message = "Tipo de ubicación no encontrado."
                });
            }

            return Ok(tipo);
        }

        // ==========================================
        // CREATE
        // ==========================================

        [HttpPost]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Create(
            [FromBody] TipoUbicacionCreateDto dto,
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
            [FromBody] TipoUbicacionUpdateDto dto,
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
                if (result.Message == "Tipo de ubicación no encontrado.")
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
            [FromBody] TipoUbicacionEstatusDto dto,
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
                return NotFound(new
                {
                    message = result.Message
                });
            }

            return Ok(new
            {
                message = dto.Activo
                    ? "Tipo de ubicación activado correctamente."
                    : "Tipo de ubicación desactivado correctamente."
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
                return NotFound(new
                {
                    message = result.Message
                });
            }

            return NoContent();
        }
    }
}
