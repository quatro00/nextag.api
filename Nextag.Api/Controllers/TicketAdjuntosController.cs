using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Services.Archivos.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/tickets/{ticketId:guid}/adjuntos")]
    [Authorize(
        Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
    [RequiereEmpresa]
    public class TicketAdjuntosController : ControllerBase
    {
        private readonly IArchivoAdjuntoService service;

        public TicketAdjuntosController(
            IArchivoAdjuntoService service)
        {
            this.service = service;
        }

        private bool EsAdministrador()
        {
            return User.IsInRole("ADMINISTRADOR") ||
                   User.IsInRole("ADMIN_EMPRESA");
        }

        // ==========================================
        // SUBIR ARCHIVO
        // ==========================================

        [HttpPost]
        [Authorize(
            Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Subir(
            Guid ticketId,
            IFormFile archivo,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();
            var usuarioId = User.GetUsuarioId();

            var resultado = await service.SubirInternoAsync(
                empresaId,
                ticketId,
                usuarioId,
                EsAdministrador(),
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

        // ==========================================
        // LISTAR ARCHIVOS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Listar(
            Guid ticketId,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();
            var usuarioId = User.GetUsuarioId();

            try
            {
                var archivos = await service.ListarAsync(
                    empresaId,
                    ticketId,
                    usuarioId,
                    EsAdministrador(),
                    ct);

                return Ok(archivos);
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, new
                {
                    message = "No tienes permiso para consultar los archivos de este ticket."
                });
            }
        }

        // ==========================================
        // DESCARGAR ARCHIVO
        // ==========================================

        [HttpGet("{archivoId:guid}")]
        public async Task<IActionResult> Descargar(
            Guid ticketId,
            Guid archivoId,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();
            var usuarioId = User.GetUsuarioId();

            try
            {
                var archivo = await service.DescargarAsync(
                    empresaId,
                    ticketId,
                    archivoId,
                    usuarioId,
                    EsAdministrador(),
                    ct);

                if (archivo == null)
                {
                    return NotFound(new
                    {
                        message = "Archivo no encontrado."
                    });
                }

                return File(
                    archivo.Contenido,
                    archivo.MimeType,
                    archivo.NombreArchivo);
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, new
                {
                    message = "No tienes permiso para descargar este archivo."
                });
            }
        }

        // ==========================================
        // ELIMINAR ARCHIVO
        // ==========================================

        [HttpDelete("{archivoId:guid}")]
        [Authorize(
            Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR")]
        public async Task<IActionResult> Eliminar(
            Guid ticketId,
            Guid archivoId,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();
            var usuarioId = User.GetUsuarioId();

            try
            {
                var eliminado = await service.EliminarAsync(
                    empresaId,
                    ticketId,
                    archivoId,
                    usuarioId,
                    EsAdministrador(),
                    ct);

                if (!eliminado)
                {
                    return NotFound(new
                    {
                        message = "Archivo no encontrado."
                    });
                }

                return Ok(new
                {
                    message = "Archivo eliminado correctamente."
                });
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, new
                {
                    message = "No tienes permiso para eliminar este archivo."
                });
            }
        }
    }
}
