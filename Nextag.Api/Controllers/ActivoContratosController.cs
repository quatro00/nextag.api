using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Services.ContratoActivos.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/activos/{activoId:guid}/contratos")]
    [Authorize(Roles =
       "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
    [RequiereEmpresa]
    public class ActivoContratosController : ControllerBase
    {
        private readonly IContratoActivoService service;

        public ActivoContratosController(
            IContratoActivoService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            Guid activoId,
            [FromQuery] bool incluirHistorial = false,
            CancellationToken ct = default)
        {
            var result = await service.GetContratosPorActivoAsync(
                activoId,
                HttpContext.GetEmpresaId(),
                incluirHistorial,
                ct);

            if (result.Success)
            {
                return Ok(result.Data);
            }

            if (result.Message == "Activo no encontrado.")
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
