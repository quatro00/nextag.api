using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Nextag.Api.Models.Dto.Tickets.Publicos;
using Nextag.Api.Services.Archivos.Interface;
using Nextag.Api.Services.Tickets.Interface;
using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/publico/etiquetas")]
    [AllowAnonymous]
    public class TicketsPublicosController : ControllerBase
    {
        private readonly ITicketPublicoService service;
        private readonly IArchivoAdjuntoService archivoService;

        public TicketsPublicosController(
        ITicketPublicoService service,
        IArchivoAdjuntoService archivoService)
        {
            this.service = service;
            this.archivoService = archivoService;
        }

        // ==========================================
        // CATEGORIAS PUBLICAS
        // ==========================================

        [HttpGet("{tokenPublico}/categorias-ticket")]
        public async Task<IActionResult> GetCategorias(
            string tokenPublico,
            CancellationToken ct)
        {
            Response.Headers.CacheControl = "no-store";

            var categorias = await service.GetCategoriasAsync(
                tokenPublico,
                ct);

            if (categorias == null)
            {
                return NotFound(new
                {
                    message =
                        "La etiqueta no permite registrar tickets públicos."
                });
            }

            return Ok(categorias);
        }

        // ==========================================
        // REGISTRAR TICKET
        // ==========================================

        [HttpPost("{tokenPublico}/tickets")]
        [EnableRateLimiting("TicketsPublicos")]
        public async Task<IActionResult> Crear(
            string tokenPublico,
            [FromBody] TicketPublicoCrearDto dto,
            CancellationToken ct)
        {
            Response.Headers.CacheControl = "no-store";

            try
            {
                var resultado = await service.CrearAsync(
                    tokenPublico,
                    dto,
                    ct);

                if (resultado == null)
                {
                    return NotFound(new
                    {
                        message =
                            "La etiqueta no permite registrar tickets públicos."
                    });
                }

                return StatusCode(
                    StatusCodes.Status201Created,
                    resultado);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // ==========================================
        // SUBIR ADJUNTO DE TICKET PUBLICO
        // ==========================================

        [HttpPost("/api/publico/tickets/{ticketId:guid}/adjuntos")]
        [EnableRateLimiting("TicketsPublicos")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(11 * 1024 * 1024)]
        public async Task<IActionResult> SubirAdjunto(
            Guid ticketId,
            IFormFile archivo,
            [FromForm] string token,
            CancellationToken ct)
        {
            Response.Headers.CacheControl = "no-store";

            // ======================================
            // VALIDACIONES BASICAS
            // ======================================

            if (ticketId == Guid.Empty)
            {
                return BadRequest(new
                {
                    message = "El ticket no es válido."
                });
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new
                {
                    message = "La autorización de carga es obligatoria."
                });
            }

            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest(new
                {
                    message = "Debes seleccionar un archivo válido."
                });
            }

            // ======================================
            // GUARDAR ARCHIVO
            // ======================================

            var resultado = await archivoService
                .SubirPublicoAsync(
                    ticketId,
                    token,
                    archivo,
                    ct);

            if (!resultado.Success)
            {
                return BadRequest(new
                {
                    message = resultado.Message
                });
            }

            return Ok(resultado.Archivo);
        }
    }
}
