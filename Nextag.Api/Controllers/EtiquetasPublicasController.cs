using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Services.EtiquetasPublicas.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/publico/etiquetas")]
    [AllowAnonymous]
    public class EtiquetasPublicasController : ControllerBase
    {
        private readonly IEtiquetaPublicaService service;

        public EtiquetasPublicasController(
            IEtiquetaPublicaService service)
        {
            this.service = service;
        }

        [HttpGet("{tokenPublico}")]
        public async Task<IActionResult> Consultar(
            string tokenPublico,
            [FromQuery] string? origen,
            CancellationToken ct)
        {
            var ip = HttpContext.Connection
                .RemoteIpAddress?
                .ToString();

            var userAgent = Request.Headers.UserAgent.ToString();

            var resultado = await service.ConsultarAsync(
                tokenPublico,
                origen,
                ip,
                userAgent,
                ct);

            if (resultado is null)
            {
                return NotFound(new
                {
                    message = "Etiqueta no encontrada o no disponible."
                });
            }

            Response.Headers.CacheControl = "no-store";

            return Ok(resultado);
        }
    }
}
