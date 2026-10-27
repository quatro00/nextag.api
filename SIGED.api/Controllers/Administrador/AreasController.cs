using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIGED.api.Models.Dto.Administrador.Areas;
using SIGED.api.Services.Administrador.Interface;

namespace SIGED.api.Controllers.Administrador
{
    [ApiController]
    [Route("api/administrador/areas")]
    [Authorize(Roles = "ADMINISTRADOR")]
    public class AreasController : ControllerBase
    {
        private readonly IAreaAdministradorService service;

        public AreasController(IAreaAdministradorService service)
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
            if (item == null) return NotFound(new { message = "Área no encontrada." });
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AreaCreateDto dto)
        {
            var result = await service.CreateAsync(dto);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AreaUpdateDto dto)
        {
            var result = await service.UpdateAsync(id, dto);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result.Data);
        }

        [HttpPatch("{id:guid}/estatus")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] AreaEstatusDto dto)
        {
            var result = await service.UpdateStatusAsync(id, dto.Activo);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(new { message = dto.Activo ? "Área activada correctamente." : "Área desactivada correctamente." });
        }
    }
}
