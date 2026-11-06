using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.Proveedores;
using Nextag.Api.Services.ProveedorContactos.Interface;

namespace Nextag.Api.Controllers;

[ApiController]
[Route("api/proveedores/{proveedorId:guid}/contactos")]
[Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
[RequiereEmpresa]
public class ProveedorContactosController : ControllerBase
{
    private readonly IProveedorContactoService service;
    public ProveedorContactosController(IProveedorContactoService service) => this.service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        Guid proveedorId, [FromQuery] bool? activo, CancellationToken ct)
    {
        var contactos = await service.GetAllAsync(proveedorId, HttpContext.GetEmpresaId(),
            activo, ct);
        return contactos is null
            ? NotFound(new { message = "Proveedor no encontrado." })
            : Ok(contactos);
    }

    [HttpGet("{contactoId:guid}")]
    public async Task<IActionResult> GetById(
        Guid proveedorId, Guid contactoId, CancellationToken ct)
    {
        var contacto = await service.GetByIdAsync(proveedorId, contactoId,
            HttpContext.GetEmpresaId(), ct);
        return contacto is null
            ? NotFound(new { message = "Contacto no encontrado." })
            : Ok(contacto);
    }

    [HttpPost]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
    public async Task<IActionResult> Create(
        Guid proveedorId, [FromBody] ProveedorContactoGuardarDto dto,
        CancellationToken ct)
    {
        var result = await service.CreateAsync(proveedorId, dto,
            HttpContext.GetEmpresaId(), User.GetUsuarioId(), ct);
        if (!result.Success) return Error(result.Message);
        return CreatedAtAction(nameof(GetById),
            new { proveedorId, contactoId = result.Data!.Id }, result.Data);
    }

    [HttpPut("{contactoId:guid}")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
    public async Task<IActionResult> Update(
        Guid proveedorId, Guid contactoId,
        [FromBody] ProveedorContactoGuardarDto dto, CancellationToken ct)
    {
        var result = await service.UpdateAsync(proveedorId, contactoId, dto,
            HttpContext.GetEmpresaId(), User.GetUsuarioId(), ct);
        return result.Success ? Ok(result.Data) : Error(result.Message);
    }

    [HttpPatch("{contactoId:guid}/estatus")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
    public async Task<IActionResult> UpdateStatus(
        Guid proveedorId, Guid contactoId,
        [FromBody] ProveedorEstatusDto dto, CancellationToken ct)
    {
        var activo = dto.Activo!.Value;
        var result = await service.UpdateStatusAsync(proveedorId, contactoId,
            HttpContext.GetEmpresaId(), activo, User.GetUsuarioId(), ct);
        return result.Success
            ? Ok(new { message = activo ? "Contacto activado." : "Contacto desactivado." })
            : Error(result.Message);
    }

    [HttpDelete("{contactoId:guid}")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
    public async Task<IActionResult> Delete(
        Guid proveedorId, Guid contactoId, CancellationToken ct)
    {
        var result = await service.UpdateStatusAsync(proveedorId, contactoId,
            HttpContext.GetEmpresaId(), false, User.GetUsuarioId(), ct);
        return result.Success ? NoContent() : Error(result.Message);
    }

    private IActionResult Error(string? message) =>
        message is "Proveedor no encontrado." or "Contacto no encontrado."
            ? NotFound(new { message })
            : BadRequest(new { message });
}
