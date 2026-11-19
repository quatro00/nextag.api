using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.Notificaciones;
using Nextag.Api.Services.Notificaciones.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/notificaciones/preferencias")]
    [Authorize(
       Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
    [RequiereEmpresa]
    public class NotificacionPreferenciasController
       : ControllerBase
    {
        private readonly INotificacionPreferenciaService service;

        public NotificacionPreferenciasController(
            INotificacionPreferenciaService service)
        {
            this.service = service;
        }

        // ==========================================
        // OBTENER PREFERENCIAS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetAll(
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();

            var usuarioId = User.GetUsuarioId();

            try
            {
                var resultado = await service.ObtenerAsync(
                    empresaId,
                    usuarioId,
                    ct);

                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new
                {
                    message = ex.Message
                });
            }
        }

        // ==========================================
        // GUARDAR PREFERENCIAS
        // ==========================================

        [HttpPut("{eventoId:int}")]
        public async Task<IActionResult> Guardar(
            int eventoId,
            [FromBody] NotificacionPreferenciaGuardarDto dto,
            CancellationToken ct)
        {
            var empresaId = HttpContext.GetEmpresaId();

            var usuarioId = User.GetUsuarioId();

            try
            {
                var resultado = await service.GuardarAsync(
                    empresaId,
                    usuarioId,
                    eventoId,
                    dto,
                    ct);

                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new
                {
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (DbUpdateException ex)
                when (
                    ex.InnerException is SqlException sql &&
                    (sql.Number == 2601 || sql.Number == 2627)
                )
            {
                return Conflict(new
                {
                    message =
                        "Las preferencias fueron modificadas simultáneamente. Recarga la configuración e intenta nuevamente."
                });
            }
        }
    }
}
