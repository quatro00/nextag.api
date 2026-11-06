using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.Etiquetas;
using Nextag.Api.Services.ActivoEtiquetas.Interface;

namespace Nextag.Api.Controllers;

[ApiController]
[Route("api/activos/{activoId:guid}/etiquetas")]
[Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
[RequiereEmpresa]
public class ActivoEtiquetasController : ControllerBase
{
    private readonly IActivoEtiquetaService service;
    public ActivoEtiquetasController(IActivoEtiquetaService service) => this.service = service;

    [HttpGet]
    public async Task<IActionResult> GetHistorial(Guid activoId, CancellationToken ct)
    {
        var items = await service.GetHistorialAsync(activoId, HttpContext.GetEmpresaId(), ct);
        return items is null
            ? NotFound(new { message = "Activo no encontrado." })
            : Ok(items);
    }

    [HttpGet("vigente")]
    public async Task<IActionResult> GetVigente(Guid activoId, CancellationToken ct)
    {
        var item = await service.GetVigenteAsync(activoId, HttpContext.GetEmpresaId(), ct);
        return item is null
            ? NotFound(new { message = "El activo no tiene etiqueta vigente o no existe." })
            : Ok(item);
    }

    [HttpPost("vincular")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
    public async Task<IActionResult> Vincular(
        Guid activoId, [FromBody] EtiquetaVincularDto dto, CancellationToken ct)
    {
        var result = await service.VincularAsync(
            activoId, dto, HttpContext.GetEmpresaId(), User.GetUsuarioId(), ct);
        return result.Success ? Ok(result.Data) : Error(result.Message);
    }

    [HttpPost("reemplazar")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
    public async Task<IActionResult> Reemplazar(
        Guid activoId, [FromBody] EtiquetaReemplazarDto dto, CancellationToken ct)
    {
        var result = await service.ReemplazarAsync(
            activoId, dto, HttpContext.GetEmpresaId(), User.GetUsuarioId(), ct);
        return result.Success ? Ok(result.Data) : Error(result.Message);
    }

    // La desvinculación anula también la etiqueta; nunca podrá reutilizarse.
    [HttpPost("desvincular")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
    public async Task<IActionResult> Desvincular(
        Guid activoId, [FromBody] EtiquetaDesvincularDto dto, CancellationToken ct)
    {
        var result = await service.DesvincularAsync(
            activoId, dto, HttpContext.GetEmpresaId(), User.GetUsuarioId(), ct);
        return result.Success ? NoContent() : Error(result.Message);
    }

    private IActionResult Error(string? mensaje) =>
        mensaje is "Activo no encontrado." or "Etiqueta no encontrada."
            ? NotFound(new { message = mensaje })
            : BadRequest(new { message = mensaje });
}
