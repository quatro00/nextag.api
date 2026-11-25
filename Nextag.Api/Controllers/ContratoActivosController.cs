using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.ContratoActivos;
using Nextag.Api.Models.Dto;
using Nextag.Api.Services.ContratoActivos.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/contratos/{contratoId:guid}/activos")]
    [Authorize(Roles =
        "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
    [RequiereEmpresa]
    public class ContratoActivosController : ControllerBase
    {
        private readonly IContratoActivoService service;

        private const string RolesEscritura =
            "ADMINISTRADOR,ADMIN_EMPRESA";

        public ContratoActivosController(
            IContratoActivoService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            Guid contratoId,
            CancellationToken ct)
        {
            var result = await service.GetAllAsync(
                contratoId,
                HttpContext.GetEmpresaId(),
                incluirHistorial: false,
                ct: ct);

            return ResolverResultado(result);
        }

        [HttpGet("historial")]
        public async Task<IActionResult> GetHistorial(
            Guid contratoId,
            CancellationToken ct)
        {
            var result = await service.GetAllAsync(
                contratoId,
                HttpContext.GetEmpresaId(),
                incluirHistorial: true,
                ct: ct);

            return ResolverResultado(result);
        }

        [HttpGet("disponibles")]
        [Authorize(Roles = RolesEscritura)]
        public async Task<IActionResult> GetDisponibles(
            Guid contratoId,
            [FromQuery] ContratoActivoFiltroDto filtro,
            CancellationToken ct)
        {
            var result = await service.GetDisponiblesAsync(
                contratoId,
                HttpContext.GetEmpresaId(),
                filtro,
                ct);

            return ResolverResultado(result);
        }

        [HttpPost("asociar")]
        [Authorize(Roles = RolesEscritura)]
        public async Task<IActionResult> Asociar(
            Guid contratoId,
            [FromBody] ContratoActivoAsociarDto dto,
            CancellationToken ct)
        {
            var result = await service.AsociarAsync(
                contratoId,
                dto,
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
                ct);

            return ResolverResultado(result);
        }

        [HttpPatch("{id:guid}/excluir")]
        [Authorize(Roles = RolesEscritura)]
        public async Task<IActionResult> Excluir(
            Guid contratoId,
            Guid id,
            [FromBody] ContratoActivoExcluirDto dto,
            CancellationToken ct)
        {
            var result = await service.ExcluirAsync(
                contratoId,
                id,
                dto,
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
                ct);

            return ResolverResultado(result);
        }

        private IActionResult ResolverResultado<T>(
            ServiceResult<T> result)
        {
            if (result.Success)
            {
                return Ok(result.Data);
            }

            if (result.Message is
                "Contrato no encontrado." or
                "Activo no encontrado." or
                "Asociación no encontrada.")
            {
                return NotFound(new
                {
                    message = result.Message
                });
            }

            if (result.Message ==
                "La información cambió durante la operación. Vuelve a consultar.")
            {
                return Conflict(new
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
