using Nextag.Api.Models.Dto.Responsables;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.Responsables.Interface
{
    public interface IResponsableService
    {
        Task<ResponsablePaginaDto> GetAllAsync(
            Guid empresaId,
            ResponsableFiltroDto filtro,
            CancellationToken ct = default);

        Task<ResponsableDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default);

        Task<ServiceResult<ResponsableDto>> CreateAsync(
            ResponsableGuardarDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);

        Task<ServiceResult<ResponsableDto>> UpdateAsync(
            Guid id,
            ResponsableGuardarDto dto,
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
