using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Attributes;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Dto.Activos;
using Nextag.Api.Services.Activos.Interface;

namespace Nextag.Api.Controllers;

[ApiController]
[Route("api/activos")]
[Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
[RequiereEmpresa]
public class ActivosController : ControllerBase
{
    private readonly IActivoService service;

    public ActivosController(IActivoService service) => this.service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] ActivoFiltroDto filtros, CancellationToken ct)
    {
        return Ok(await service.GetAllAsync(HttpContext.GetEmpresaId(), filtros, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var activo = await service.GetByIdAsync(id, HttpContext.GetEmpresaId(), ct);
        return activo is null
            ? NotFound(new { message = "Activo no encontrado." })
            : Ok(activo);
    }

    [HttpPost]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
    public async Task<IActionResult> Create(
        [FromBody] ActivoGuardarDto dto, CancellationToken ct)
    {
        var result = await service.CreateAsync(
            dto, HttpContext.GetEmpresaId(), User.GetUsuarioId(), ct);
        if (!result.Success) return BadRequest(new { message = result.Message });

        return CreatedAtAction(nameof(GetById),
            new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] ActivoGuardarDto dto, CancellationToken ct)
    {
        var result = await service.UpdateAsync(
            id, dto, HttpContext.GetEmpresaId(), User.GetUsuarioId(), ct);

        if (!result.Success)
        {
            if (result.Message == "Activo no encontrado.")
                return NotFound(new { message = result.Message });
            return BadRequest(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    // No se expone DELETE ni cambio libre de estado.
    // Las bajas, asignaciones, traslados y mantenimientos serán operaciones
    // de negocio con historial en ActivoMovimiento.
}
