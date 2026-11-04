using Nextag.Api.Models.Dto.Sucursales;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.Sucursales.Interface
{
    public interface ISucursalService
    {
        Task<List<SucursalListDto>> GetAllAsync(
            Guid empresaId,
            bool? activo = null,
            CancellationToken ct = default);

        Task<SucursalDetalleDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default);

        Task<ServiceResult<SucursalDetalleDto>> CreateAsync(
            SucursalCreateDto dto,
            Guid empresaId,
            Guid usuarioCreacion,
            CancellationToken ct = default);

        Task<ServiceResult<SucursalDetalleDto>> UpdateAsync(
            Guid id,
            SucursalUpdateDto dto,
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
