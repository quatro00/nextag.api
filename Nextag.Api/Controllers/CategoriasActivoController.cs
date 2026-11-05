using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.CategoriasActivo;
using Nextag.Api.Services.CategoriasActivo.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/categorias-activo")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
    [RequiereEmpresa]
    public class CategoriasActivoController : ControllerBase
    {
        private readonly ICategoriaActivoService service;

        public CategoriasActivoController(
            ICategoriaActivoService service)
        {
            this.service = service;
        }

        // ==========================================
        // LISTADO
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] bool? activo,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();

            var categorias = await service.GetAllAsync(
                empresaId,
                activo,
                ct);

            return Ok(categorias);
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

            var categoria = await service.GetByIdAsync(
                id,
                empresaId,
                ct);

            if (categoria == null)
            {
                return NotFound(new
                {
                    message = "Categoría no encontrada."
                });
            }

            return Ok(categoria);
        }

        // ==========================================
        // CREAR
        // ==========================================

        [HttpPost]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Create(
            [FromBody] CategoriaActivoCreateDto dto,
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
            [FromBody] CategoriaActivoUpdateDto dto,
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
                if (result.Message == "Categoría no encontrada.")
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
        // CAMBIAR ESTATUS
        // ==========================================

        [HttpPatch("{id:guid}/estatus")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] CategoriaActivoEstatusDto dto,
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
                if (result.Message == "Categoría no encontrada.")
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
                    ? "Categoría activada correctamente."
                    : "Categoría desactivada correctamente."
            });
        }

        // ==========================================
        // ELIMINACIÓN LÓGICA
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
                if (result.Message == "Categoría no encontrada.")
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
