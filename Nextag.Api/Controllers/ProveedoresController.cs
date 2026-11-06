using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.Proveedores;
using Nextag.Api.Services.Proveedores.Interface;

namespace Nextag.Api.Controllers;

[ApiController]
[Route("api/proveedores")]
[Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
[RequiereEmpresa]
public class ProveedoresController : ControllerBase
{
    private readonly IProveedorService service;
    public ProveedoresController(IProveedorService service) => this.service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? activo,
        [FromQuery] int? tipoProveedorId,
        [FromQuery] string? buscar,
        CancellationToken ct)
    {
        var empresaId = HttpContext.GetEmpresaId();
        return Ok(await service.GetAllAsync(empresaId, activo, tipoProveedorId, buscar, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var detalle = await service.GetByIdAsync(id, HttpContext.GetEmpresaId(), ct);
        return detalle is null
            ? NotFound(new { message = "Proveedor no encontrado." })
            : Ok(detalle);
    }

    [HttpPost]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
    public async Task<IActionResult> Create(
        [FromBody] ProveedorGuardarDto dto, CancellationToken ct)
    {
        var result = await service.CreateAsync(dto, HttpContext.GetEmpresaId(),
            User.GetUsuarioId(), ct);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] ProveedorGuardarDto dto, CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, dto, HttpContext.GetEmpresaId(),
            User.GetUsuarioId(), ct);
        if (!result.Success)
        {
            if (result.Message == "Proveedor no encontrado.")
                return NotFound(new { message = result.Message });
            return BadRequest(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    [HttpPatch("{id:guid}/estatus")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
    public async Task<IActionResult> UpdateStatus(
        Guid id, [FromBody] ProveedorEstatusDto dto, CancellationToken ct)
    {
        var activo = dto.Activo!.Value;
        var result = await service.UpdateStatusAsync(id, HttpContext.GetEmpresaId(),
            activo, User.GetUsuarioId(), ct);
        if (!result.Success)
            return NotFound(new { message = result.Message });
        return Ok(new { message = activo ? "Proveedor activado." : "Proveedor desactivado." });
    }

    // Baja lógica, usando la misma regla del PATCH.
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await service.UpdateStatusAsync(id, HttpContext.GetEmpresaId(),
            false, User.GetUsuarioId(), ct);
        if (!result.Success)
            return NotFound(new { message = result.Message });
        return NoContent();
    }
}
