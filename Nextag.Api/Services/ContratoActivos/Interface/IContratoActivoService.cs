using Nextag.Api.Models.Dto.ContratoActivos;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.ContratoActivos.Interface
{
    public interface IContratoActivoService
    {
        Task<ServiceResult<List<ContratoActivoDto>>> GetAllAsync(
            Guid contratoId,
            Guid empresaId,
            bool incluirHistorial = false,
            CancellationToken ct = default);

        Task<ServiceResult<ContratoActivoDisponibleListadoDto>>
            GetDisponiblesAsync(
                Guid contratoId,
                Guid empresaId,
                ContratoActivoFiltroDto filtro,
                CancellationToken ct = default);

        Task<ServiceResult<List<ContratoActivoDto>>> AsociarAsync(
            Guid contratoId,
            ContratoActivoAsociarDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);

        Task<ServiceResult<ContratoActivoDto>> ExcluirAsync(
            Guid contratoId,
            Guid id,
            ContratoActivoExcluirDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);

        Task<ServiceResult<List<ActivoContratoDto>>>
            GetContratosPorActivoAsync(
                Guid activoId,
                Guid empresaId,
                bool incluirHistorial = false,
                CancellationToken ct = default);
    }
}
