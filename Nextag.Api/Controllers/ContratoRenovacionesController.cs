using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.ContratoRenovaciones;
using Nextag.Api.Models.Dto;
using Nextag.Api.Services.ContratoRenovaciones.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/contratos/{contratoId:guid}/renovaciones")]
    [Authorize(Roles =
        "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
    [RequiereEmpresa]
    public class ContratoRenovacionesController
        : ControllerBase
    {
        private readonly IContratoRenovacionService service;

        private const string RolesEscritura =
            "ADMINISTRADOR,ADMIN_EMPRESA";

        public ContratoRenovacionesController(
            IContratoRenovacionService service)
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
                ct);

            return ResolverResultado(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid contratoId,
            Guid id,
            CancellationToken ct)
        {
            var result = await service.GetByIdAsync(
                contratoId,
                id,
                HttpContext.GetEmpresaId(),
                ct);

            return ResolverResultado(result);
        }

        [HttpGet("informacion")]
        [Authorize(Roles = RolesEscritura)]
        public async Task<IActionResult> GetInformacion(
            Guid contratoId,
            CancellationToken ct)
        {
            var result =
                await service.GetInformacionAsync(
                    contratoId,
                    HttpContext.GetEmpresaId(),
                    ct);

            return ResolverResultado(result);
        }

        [HttpPost]
        [Authorize(Roles = RolesEscritura)]
        public async Task<IActionResult> Renovar(
            Guid contratoId,
            [FromBody] ContratoRenovacionGuardarDto dto,
            CancellationToken ct)
        {
            var result = await service.RenovarAsync(
                contratoId,
                dto,
                HttpContext.GetEmpresaId(),
                User.GetUsuarioId(),
                ct);

            if (!result.Success)
            {
                return ResolverResultado(result);
            }

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    contratoId = contratoId,

                    id = result.Data!.Renovacion.Id
                },
                result.Data);
        }

        private IActionResult ResolverResultado<T>(
            ServiceResult<T> result)
        {
            if (result.Success)
            {
                return Ok(result.Data);
            }

            if (
                result.Message is
                    "Contrato no encontrado." or
                    "Renovación no encontrada."
            )
            {
                return NotFound(new
                {
                    message = result.Message
                });
            }

            if (
                result.Message ==
                    "La información del contrato cambió durante la operación. Vuelve a consultar."
            )
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
