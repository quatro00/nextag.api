using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.Sucursales;
using Nextag.Api.Services.Sucursales.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/sucursales")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
    [RequiereEmpresa]
    public class SucursalesController : ControllerBase
    {
        private readonly ISucursalService service;

        public SucursalesController(ISucursalService service)
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

            var sucursales = await service.GetAllAsync(
                empresaId,
                activo,
                ct);

            return Ok(sucursales);
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

            var sucursal = await service.GetByIdAsync(
                id,
                empresaId,
                ct);

            if (sucursal == null)
            {
                return NotFound(new
                {
                    message = "Sucursal no encontrada."
                });
            }

            return Ok(sucursal);
        }

        // ==========================================
        // CREATE
        // ==========================================

        [HttpPost]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Create(
            [FromBody] SucursalCreateDto dto,
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
            [FromBody] SucursalUpdateDto dto,
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
                if (result.Message == "Sucursal no encontrada.")
                    return NotFound(new { message = result.Message });

                return BadRequest(new { message = result.Message });
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
            [FromBody] SucursalEstatusDto dto,
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
                    ? "Sucursal activada correctamente."
                    : "Sucursal desactivada correctamente."
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
