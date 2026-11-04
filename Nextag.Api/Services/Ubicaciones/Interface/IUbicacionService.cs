using Nextag.Api.Models.Dto.Ubicaciones;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.Ubicaciones.Interface
{
    public interface IUbicacionService
    {
        Task<List<UbicacionListDto>> GetAllAsync(
            Guid empresaId,
            Guid? sucursalId = null,
            bool? activo = null,
            CancellationToken ct = default);

        Task<UbicacionDetalleDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default);

        Task<ServiceResult<UbicacionDetalleDto>> CreateAsync(
            UbicacionCreateDto dto,
            Guid empresaId,
            Guid usuarioCreacion,
            CancellationToken ct = default);

        Task<ServiceResult<UbicacionDetalleDto>> UpdateAsync(
            Guid id,
            UbicacionUpdateDto dto,
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
