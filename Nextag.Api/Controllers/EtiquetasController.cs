using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.Etiquetas;
using Nextag.Api.Services.Etiquetas.Interface;

namespace Nextag.Api.Controllers;

[ApiController]
[Route("api/etiquetas")]
[Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
[RequiereEmpresa]
public class EtiquetasController : ControllerBase
{
    private readonly IEtiquetaService service;
    public EtiquetasController(IEtiquetaService service) => this.service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] EtiquetaFiltroDto filtros, CancellationToken ct)
    {
        var estatus = filtros.Estatus?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(estatus) &&
            estatus is not ("DISPONIBLE" or "VINCULADA" or "RETIRADA"))
            return BadRequest(new { message = "Estatus inválido. Usa DISPONIBLE, VINCULADA o RETIRADA." });

        return Ok(await service.GetAllAsync(HttpContext.GetEmpresaId(), filtros, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var etiqueta = await service.GetByIdAsync(id, HttpContext.GetEmpresaId(), ct);
        return etiqueta is null
            ? NotFound(new { message = "Etiqueta no encontrada." })
            : Ok(etiqueta);
    }

    // Generación individual y por lote (1..250), antes de imprimir o vincular.
    [HttpPost("generar")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
    public async Task<IActionResult> Generar(
        [FromBody] EtiquetaGenerarDto dto, CancellationToken ct)
    {
        var result = await service.GenerarAsync(
            dto, HttpContext.GetEmpresaId(), User.GetUsuarioId(), ct);
        return result.Success
            ? Ok(result.Data)
            : BadRequest(new { message = result.Message });
    }

    // Se registra UID y/o bloqueo NFC ya COMPROBADO físicamente.
    [HttpPatch("{id:guid}/nfc")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
    public async Task<IActionResult> ActualizarNfc(
        Guid id, [FromBody] EtiquetaNfcDto dto, CancellationToken ct)
    {
        var result = await service.ActualizarNfcAsync(
            id, dto, HttpContext.GetEmpresaId(), User.GetUsuarioId(), ct);
        if (!result.Success)
            return Error(result.Message);
        return Ok(result.Data);
    }

    [HttpPost("{id:guid}/anular")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
    public async Task<IActionResult> Anular(
        Guid id, [FromBody] EtiquetaAnularDto dto, CancellationToken ct)
    {
        var result = await service.AnularAsync(
            id, dto, HttpContext.GetEmpresaId(), User.GetUsuarioId(), ct);
        if (!result.Success)
            return Error(result.Message);
        return Ok(new { message = "Etiqueta anulada correctamente." });
    }

    private IActionResult Error(string? mensaje) =>
        mensaje == "Etiqueta no encontrada."
            ? NotFound(new { message = mensaje })
            : BadRequest(new { message = mensaje });
}
