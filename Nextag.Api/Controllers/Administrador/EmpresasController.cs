using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.Administrador.Empresas;
using Nextag.Api.Services.Administrador.Interface;

namespace Nextag.Api.Controllers.Administrador
{
    [ApiController]
    [Route("api/administrador/empresas")]
    [Authorize(Roles = "ADMINISTRADOR")]
    public class EmpresasController : ControllerBase
    {
        private readonly IEmpresaAdministradorService service;

        public EmpresasController(
            IEmpresaAdministradorService service)
        {
            this.service = service;
        }

        // ==========================================
        // GET ALL
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var empresas = await service.GetAllAsync();

            return Ok(empresas);
        }

        // ==========================================
        // GET BY ID
        // ==========================================

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var empresa = await service.GetByIdAsync(id);

            if (empresa == null)
            {
                return NotFound(new { message = "Empresa no encontrada." });
            }

            return Ok(empresa);
        }

        // ==========================================
        // CREATE
        // ==========================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] EmpresaCreateDto dto)
        {
            var usuarioId = User.GetUsuarioId();

            var result = await service.CreateAsync(dto, usuarioId);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }

        // ==========================================
        // UPDATE
        // ==========================================

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] EmpresaUpdateDto dto)
        {
            var usuarioId = User.GetUsuarioId();

            var result = await service.UpdateAsync(id, dto, usuarioId);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result.Data);
        }

        // ==========================================
        // UPDATE STATUS
        // ==========================================

        [HttpPatch("{id:guid}/estatus")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] EmpresaEstatusDto dto)
        {
            var usuarioId = User.GetUsuarioId();

            var result = await service.UpdateStatusAsync(id, dto.Activo, usuarioId);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = dto.Activo ? "Empresa activada correctamente." : "Empresa desactivada correctamente." });
        }

        // ==========================================
        // DELETE (LOGICO)
        // ==========================================

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var usuarioId = User.GetUsuarioId();

            var result = await service.UpdateStatusAsync(id, false, usuarioId);

            if (!result.Success)
            {
                return NotFound(new { message = result.Message });
            }

            return NoContent();
        }
    }
}
