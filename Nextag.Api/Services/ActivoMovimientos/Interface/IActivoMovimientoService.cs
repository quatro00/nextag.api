using Nextag.Api.Models.Dto.ActivoMovimientos;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.ActivoMovimientos.Interface
{
    public interface IActivoMovimientoService
    {
        Task<ActivoMovimientoPaginaDto> GetAllAsync(
            Guid empresaId,
            ActivoMovimientoFiltroDto filtros,
            CancellationToken ct = default);

        Task<ActivoMovimientoDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default);

        Task<List<ActivoMovimientoDto>> GetHistorialActivoAsync(
            Guid activoId,
            Guid empresaId,
            CancellationToken ct = default);

        Task<List<TipoMovimientoActivoDto>> GetTiposAsync(
            CancellationToken ct = default);

        Task<ActivoSituacionDto?> GetSituacionAsync(
            Guid activoId,
            Guid empresaId,
            CancellationToken ct = default);

        Task<ServiceResult<ActivoMovimientoDto>> CreateAsync(
            ActivoMovimientoCrearDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);
    }
}
