using Nextag.Api.Models.Dto.EquiposSoporte;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.EquiposSoporte.Interface
{
    public interface IEquipoSoporteService
    {
        Task<List<EquipoSoporteListDto>> GetAllAsync(
            Guid empresaId,
            bool? activo = null,
            string? tipo = null,
            string? buscar = null,
            CancellationToken ct = default);

        Task<EquipoSoporteDetalleDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default);

        Task<ServiceResult<EquipoSoporteDetalleDto>> CreateAsync(
            EquipoSoporteGuardarDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);

        Task<ServiceResult<EquipoSoporteDetalleDto>> UpdateAsync(
            Guid id,
            EquipoSoporteGuardarDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);

        Task<ServiceResult<bool>> UpdateStatusAsync(
            Guid id,
            Guid empresaId,
            bool activo,
            Guid usuarioId,
            CancellationToken ct = default);
    }
}
