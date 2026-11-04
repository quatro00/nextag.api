using Nextag.Api.Models.Dto.TiposUbicacion;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.TiposUbicacion.Interface
{
    public interface ITipoUbicacionService
    {
        Task<List<TipoUbicacionListDto>> GetAllAsync(
            Guid empresaId,
            bool? activo = null,
            CancellationToken ct = default);

        Task<TipoUbicacionDetalleDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default);

        Task<ServiceResult<TipoUbicacionDetalleDto>> CreateAsync(
            TipoUbicacionCreateDto dto,
            Guid empresaId,
            Guid usuarioCreacion,
            CancellationToken ct = default);

        Task<ServiceResult<TipoUbicacionDetalleDto>> UpdateAsync(
            Guid id,
            TipoUbicacionUpdateDto dto,
            Guid empresaId,
            Guid usuarioModificacion,
            CancellationToken ct = default);

        Task<ServiceResult<bool>> UpdateStatusAsync(
            Guid id,
            Guid empresaId,
            bool activo,
            Guid usuarioModificacion,
            CancellationToken ct = default);
    }
}
