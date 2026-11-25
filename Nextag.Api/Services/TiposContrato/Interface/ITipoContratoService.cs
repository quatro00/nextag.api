using Nextag.Api.Models.Dto.TiposContrato;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.TiposContrato.Interface
{
    public interface ITipoContratoService
    {
        Task<List<TipoContratoListDto>> GetAllAsync(
            Guid empresaId,
            bool? activo = null,
            string? buscar = null,
            CancellationToken ct = default);

        Task<TipoContratoDetalleDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default);

        Task<ServiceResult<TipoContratoDetalleDto>> CreateAsync(
            TipoContratoGuardarDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);

        Task<ServiceResult<TipoContratoDetalleDto>> UpdateAsync(
            Guid id,
            TipoContratoGuardarDto dto,
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
