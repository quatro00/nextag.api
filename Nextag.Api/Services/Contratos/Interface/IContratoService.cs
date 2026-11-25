using Nextag.Api.Models.Dto.Contratos;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.Contratos.Interface
{
    public interface IContratoService
    {
        Task<ServiceResult<ContratoListadoDto>> GetAllAsync(
            Guid empresaId,
            ContratoFiltroDto filtro,
            CancellationToken ct = default);

        Task<ContratoDetalleDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default);

        Task<ServiceResult<ContratoDetalleDto>> CreateAsync(
            ContratoGuardarDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);

        Task<ServiceResult<ContratoDetalleDto>> UpdateAsync(
            Guid id,
            ContratoGuardarDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);

        Task<ServiceResult<ContratoDetalleDto>> ActivarAsync(
            Guid id,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);

        Task<ServiceResult<ContratoDetalleDto>> SuspenderAsync(
            Guid id,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);

        Task<ServiceResult<ContratoDetalleDto>> ReanudarAsync(
            Guid id,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);

        Task<ServiceResult<ContratoDetalleDto>> CancelarAsync(
            Guid id,
            ContratoCancelarDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);

        Task<ServiceResult<ContratoDetalleDto>> FinalizarAsync(
            Guid id,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);

        Task<ServiceResult<bool>> DeleteAsync(
            Guid id,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);
    }
}
