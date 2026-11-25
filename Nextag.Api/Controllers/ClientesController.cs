using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.Clientes;
using Nextag.Api.Services.Clientes.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/clientes")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
    [RequiereEmpresa]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService service;

        public ClientesController(IClienteService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] bool? activo,
            [FromQuery] string? buscar,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();

            var clientes = await service.GetAllAsync(
                empresaId,
                activo,
                buscar,
                ct);

            return Ok(clientes);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken ct)
        {
            var detalle = await service.GetByIdAsync(
                id,
                HttpContext.GetEmpresaId(),
                ct);

            return detalle is null
                ? NotFound(new
                {
                    message = "Cliente no encontrado."
                })
                : Ok(detalle);
        }

        [HttpPost]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Create(
            [FromBody] ClienteGuardarDto dto,
            CancellationToken ct)
        {
            var result = await service.CreateAsync(
                dto,
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
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

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] ClienteGuardarDto dto,
            CancellationToken ct)
        {
            var result = await service.UpdateAsync(
                id,
                dto,
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
                ct);

            if (!result.Success)
            {
                if (result.Message == "Cliente no encontrado.")
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

        [HttpPatch("{id:guid}/estatus")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] ClienteEstatusDto dto,
            CancellationToken ct)
        {
            var activo = dto.Activo!.Value;

            var result = await service.UpdateStatusAsync(
                id,
                HttpContext.GetEmpresaId(),
                activo,
                User.GetUsuarioId(),
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
                message = activo
                    ? "Cliente activado."
                    : "Cliente desactivado."
            });
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken ct)
        {
            var result = await service.UpdateStatusAsync(
                id,
                HttpContext.GetEmpresaId(),
                false,
                User.GetUsuarioId(),
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
