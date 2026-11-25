using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.Contratos;
using Nextag.Api.Models.Dto;
using Nextag.Api.Services.Contratos.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/contratos")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
    [RequiereEmpresa]
    public class ContratosController : ControllerBase
    {
        private readonly IContratoService service;

        private const string RolesEscritura =
            "ADMINISTRADOR,ADMIN_EMPRESA";

        public ContratosController(
            IContratoService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] ContratoFiltroDto filtro,
            CancellationToken ct)
        {
            var result = await service.GetAllAsync(
                HttpContext.GetEmpresaId(),
                filtro,
                ct);

            return ResolverResultado(result);
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
                    message = "Contrato no encontrado."
                })
                : Ok(detalle);
        }

        [HttpPost]
        [Authorize(Roles = RolesEscritura)]
        public async Task<IActionResult> Create(
            [FromBody] ContratoGuardarDto dto,
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
        [Authorize(Roles = RolesEscritura)]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] ContratoGuardarDto dto,
            CancellationToken ct)
        {
            var result = await service.UpdateAsync(
                id,
                dto,
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
                ct);

            return ResolverResultado(result);
        }

        [HttpPatch("{id:guid}/activar")]
        [Authorize(Roles = RolesEscritura)]
        public async Task<IActionResult> Activar(
            Guid id,
            CancellationToken ct)
        {
            var result = await service.ActivarAsync(
                id,
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
                ct);

            return ResolverResultado(result);
        }

        [HttpPatch("{id:guid}/suspender")]
        [Authorize(Roles = RolesEscritura)]
        public async Task<IActionResult> Suspender(
            Guid id,
            CancellationToken ct)
        {
            var result = await service.SuspenderAsync(
                id,
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
                ct);

            return ResolverResultado(result);
        }

        [HttpPatch("{id:guid}/reanudar")]
        [Authorize(Roles = RolesEscritura)]
        public async Task<IActionResult> Reanudar(
            Guid id,
            CancellationToken ct)
        {
            var result = await service.ReanudarAsync(
                id,
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
                ct);

            return ResolverResultado(result);
        }

        [HttpPatch("{id:guid}/cancelar")]
        [Authorize(Roles = RolesEscritura)]
        public async Task<IActionResult> Cancelar(
            Guid id,
            [FromBody] ContratoCancelarDto dto,
            CancellationToken ct)
        {
            var result = await service.CancelarAsync(
                id,
                dto,
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
                ct);

            return ResolverResultado(result);
        }

        [HttpPatch("{id:guid}/finalizar")]
        [Authorize(Roles = RolesEscritura)]
        public async Task<IActionResult> Finalizar(
            Guid id,
            CancellationToken ct)
        {
            var result = await service.FinalizarAsync(
                id,
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
                ct);

            return ResolverResultado(result);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = RolesEscritura)]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken ct)
        {
            var result = await service.DeleteAsync(
                id,
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
                ct);

            if (!result.Success)
            {
                return ResolverResultado(result);
            }

            return NoContent();
        }

        private IActionResult ResolverResultado<T>(
            ServiceResult<T> result)
        {
            if (result.Success)
            {
                return Ok(result.Data);
            }

            if (result.Message == "Contrato no encontrado.")
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
    }
}
