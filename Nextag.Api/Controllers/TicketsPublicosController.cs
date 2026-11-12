using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Nextag.Api.Models.Dto.Tickets.Publicos;
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

        public TicketsPublicosController(
            ITicketPublicoService service)
        {
            this.service = service;
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
    }
}
