using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGED.api.Models.Dto.Administrador.Usuarios;
using SIGED.api.Services.Administrador.Interface;

namespace SIGED.api.Controllers.Administrador
{
    [ApiController]
    [Route("api/administrador/usuarios")]
    [Authorize(Roles = "ADMINISTRADOR")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioAdministradorService service;

        public UsuariosController(IUsuarioAdministradorService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await service.GetAllAsync());
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await service.GetByIdAsync(id);
            if (item == null) return NotFound(new { message = "Usuario no encontrado." });
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UsuarioCreateDto dto)
        {
            var result = await service.CreateAsync(dto);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UsuarioUpdateDto dto)
        {
            var result = await service.UpdateAsync(id, dto);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result.Data);
        }

        [HttpPatch("{id:guid}/estatus")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UsuarioEstatusDto dto)
        {
            var result = await service.UpdateStatusAsync(id, dto.Activo);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(new { message = dto.Activo ? "Usuario activado correctamente." : "Usuario desactivado correctamente." });
        }

        [HttpPut("{id:guid}/roles")]
        public async Task<IActionResult> UpdateRoles(Guid id, [FromBody] UsuarioRolesDto dto)
        {
            var result = await service.UpdateRolesAsync(id, dto.Roles);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result.Data);
        }
    }
}
