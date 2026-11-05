using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Services.EstadosActivo.Interface;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [Route("api/estados-activo")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
    public class EstadosActivoController : ControllerBase
    {
        private readonly IEstadoActivoService service;

        public EstadosActivoController(
            IEstadoActivoService service)
        {
            this.service = service;
        }

        // ==========================================
        // LISTADO
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetAll(
            CancellationToken ct)
        {
            var estados = await service.GetAllAsync(ct);

            return Ok(estados);
        }

        // ==========================================
        // DETALLE
        // ==========================================

        [HttpGet("{id:int:min(1)}")]
        public async Task<IActionResult> GetById(
            int id,
            CancellationToken ct)
        {
            var estado = await service.GetByIdAsync(
                id,
                ct);

            if (estado == null)
            {
                return NotFound(new
                {
                    message = "Estado de activo no encontrado."
                });
            }

            return Ok(estado);
        }
    }
}
