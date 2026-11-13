using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.Etiquetas.Configuracion;
using Nextag.Api.Services.EtiquetaConfiguraciones.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/etiquetas/configuracion")]

    [Authorize(
        Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]

    [RequiereEmpresa]
    public class EtiquetaConfiguracionController : ControllerBase
    {
        private readonly IEtiquetaConfiguracionService service;

        public EtiquetaConfiguracionController(
            IEtiquetaConfiguracionService service)
        {
            this.service = service;
        }

        // ==========================================
        // CONSULTAR
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Get(
            CancellationToken ct)
        {
            var empresaId =
                HttpContext.GetEmpresaId();

            var configuracion = await service.GetAsync(
                empresaId,
                ct);

            return Ok(configuracion);
        }

        // ==========================================
        // GUARDAR
        // ==========================================

        [HttpPut]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
        public async Task<IActionResult> Guardar(
            [FromBody] EtiquetaConfiguracionGuardarDto dto,
            CancellationToken ct)
        {
            var empresaId =
                HttpContext.GetEmpresaId();

            var usuarioId =
                User.GetUsuarioId();

            var resultado = await service.GuardarAsync(
                empresaId,
                usuarioId,
                dto,
                ct);

            return Ok(resultado);
        }
    }
}
