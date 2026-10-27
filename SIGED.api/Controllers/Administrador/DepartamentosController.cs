using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIGED.api.Helpers;
using SIGED.api.Models.Dto.Administrador.Departamentos;
using SIGED.api.Services.Administrador.Interface;

namespace SIGED.api.Controllers.Administrador
{
    [ApiController]
    [Route("api/administrador/departamentos")]
    [Authorize(Roles = "ADMINISTRADOR")]
    public class DepartamentosController : ControllerBase
    {
        private readonly IDepartamentoAdministradorService service;

        public DepartamentosController(IDepartamentoAdministradorService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? areaId)
        {
            return Ok(await service.GetAllAsync(areaId));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await service.GetByIdAsync(id);
            if (item == null) return NotFound(new { message = "Departamento no encontrado." });
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DepartamentoCreateDto dto)
        {
            var usuarioId = User.GetUsuarioId();
            var result = await service.CreateAsync(dto, usuarioId);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DepartamentoUpdateDto dto)
        {
            var usuarioId = User.GetUsuarioId();
            var result = await service.UpdateAsync(id, dto, usuarioId);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(result.Data);
        }

        [HttpPatch("{id:guid}/estatus")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] DepartamentoEstatusDto dto)
        {
            var usuarioId = User.GetUsuarioId();
            var result = await service.UpdateStatusAsync(id, dto.Activo, usuarioId);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new
            {
                message = dto.Activo
                    ? "Departamento activado correctamente."
                    : "Departamento desactivado correctamente."
            });
        }
    }
}
