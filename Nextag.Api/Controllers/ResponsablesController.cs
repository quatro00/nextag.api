using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.Responsables;
using Nextag.Api.Services.Responsables.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/responsables")]
    [Authorize(Roles =
    "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
    [RequiereEmpresa]
    public class ResponsablesController : ControllerBase
    {
        private readonly IResponsableService service;

        public ResponsablesController(
            IResponsableService service)
        {
            this.service = service;
        }

        // ==========================================
        // LISTADO
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] ResponsableFiltroDto filtro,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();

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
            var resultado = await service.GetByIdAsync(
                id,
                HttpContext.GetEmpresaId(),
                ct);

            if (resultado == null)
            {
                return NotFound(new
                {
                    message = "Responsable no encontrado."
                });
            }

            return Ok(resultado);
        }

        // ==========================================
        // CREAR
        // ==========================================

        [HttpPost]
        [Authorize(Roles =
            "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Create(
            [FromBody] ResponsableGuardarDto dto,
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

        // ==========================================
        // ACTUALIZAR
        // ==========================================

        [HttpPut("{id:guid}")]
        [Authorize(Roles =
            "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] ResponsableGuardarDto dto,
            CancellationToken ct)
        {
            var resultado = await service.UpdateAsync(
                id,
                dto,
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
                ct);

            if (!resultado.Success)
            {
                if (resultado.Message ==
                    "Responsable no encontrado.")
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

        // ==========================================
        // ACTIVAR / DESACTIVAR
        // ==========================================

        [HttpPatch("{id:guid}/estatus")]
        [Authorize(Roles =
            "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] ResponsableEstatusDto dto,
            CancellationToken ct)
        {
            var resultado = await service.UpdateStatusAsync(
                id,
                HttpContext.GetEmpresaId(),
                dto.Activo,
                User.GetUsuarioId(),
                ct);

            if (!resultado.Success)
            {
                if (resultado.Message ==
                    "Responsable no encontrado.")
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

            return Ok(new
            {
                message = dto.Activo
                    ? "Responsable activado correctamente."
                    : "Responsable desactivado correctamente."
            });
        }

        // ==========================================
        // BAJA LÓGICA
        // ==========================================

        [HttpDelete("{id:guid}")]
        [Authorize(Roles =
            "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken ct)
        {
            var resultado = await service.UpdateStatusAsync(
                id,
                HttpContext.GetEmpresaId(),
                false,
                User.GetUsuarioId(),
                ct);

            if (!resultado.Success)
            {
                if (resultado.Message ==
                    "Responsable no encontrado.")
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

            return NoContent();
        }
    }
}
