using Nextag.Api.Models.Dto.EquipoSoporteIntegrantes;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.EquipoSoporteIntegrantes.Interface
{
    public interface IEquipoSoporteIntegranteService
    {
        Task<List<EquipoSoporteIntegranteDto>> GetAllAsync(
            Guid empresaId,
            Guid equipoId,
            bool? activo = null,
            CancellationToken ct = default);

        Task<EquipoSoporteIntegranteDto?> GetByIdAsync(
            Guid empresaId,
            Guid equipoId,
            Guid id,
            CancellationToken ct = default);

        Task<List<UsuarioDisponibleSoporteDto>> GetDisponiblesAsync(
            Guid empresaId,
            Guid equipoId,
            string? buscar = null,
            CancellationToken ct = default);

        Task<ServiceResult<EquipoSoporteIntegranteDto>> CreateAsync(
            Guid empresaId,
            Guid equipoId,
            EquipoSoporteIntegranteCrearDto dto,
            Guid usuarioCreacion,
            CancellationToken ct = default);

        Task<ServiceResult<bool>> UpdateStatusAsync(
            Guid empresaId,
            Guid equipoId,
            Guid id,
            bool activo,
            Guid usuarioModificacion,
            CancellationToken ct = default);
    }
}
