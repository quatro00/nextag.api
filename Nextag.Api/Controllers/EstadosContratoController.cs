using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Services.EstadosContrato.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/estados-contrato")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
    [RequiereEmpresa]
    public class EstadosContratoController : ControllerBase
    {
        private readonly IEstadoContratoService service;

        public EstadosContratoController(
            IEstadoContratoService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] bool? activo,
            CancellationToken ct)
        {
            return Ok(await service.GetAllAsync(
                activo,
                ct));
        }
    }
}
