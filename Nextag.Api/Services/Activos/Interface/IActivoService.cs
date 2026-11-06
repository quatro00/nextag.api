using Nextag.Api.Models.Dto;
using Nextag.Api.Models.Dto.Activos;

namespace Nextag.Api.Services.Activos.Interface;

public interface IActivoService
{
    Task<ActivoPaginaDto> GetAllAsync(
        Guid empresaId, ActivoFiltroDto filtros, CancellationToken ct = default);

    Task<ActivoDetalleDto?> GetByIdAsync(
        Guid id, Guid empresaId, CancellationToken ct = default);

    Task<ServiceResult<ActivoDetalleDto>> CreateAsync(
        ActivoGuardarDto dto, Guid empresaId, Guid usuarioId,
        CancellationToken ct = default);

    Task<ServiceResult<ActivoDetalleDto>> UpdateAsync(
        Guid id, ActivoGuardarDto dto, Guid empresaId, Guid usuarioId,
        CancellationToken ct = default);
}
