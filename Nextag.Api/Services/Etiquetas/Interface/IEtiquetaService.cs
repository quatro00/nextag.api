using Nextag.Api.Models.Dto;
using Nextag.Api.Models.Dto.Etiquetas;

namespace Nextag.Api.Services.Etiquetas.Interface;

public interface IEtiquetaService
{
    Task<EtiquetaPaginaDto> GetAllAsync(Guid empresaId, EtiquetaFiltroDto filtros, CancellationToken ct = default);
    Task<EtiquetaDto?> GetByIdAsync(Guid id, Guid empresaId, CancellationToken ct = default);
    Task<ServiceResult<List<EtiquetaDto>>> GenerarAsync(
        EtiquetaGenerarDto dto, Guid empresaId, Guid usuarioId, CancellationToken ct = default);
    Task<ServiceResult<EtiquetaDto>> ActualizarNfcAsync(
        Guid id, EtiquetaNfcDto dto, Guid empresaId, Guid usuarioId, CancellationToken ct = default);
    Task<ServiceResult<bool>> AnularAsync(
        Guid id, EtiquetaAnularDto dto, Guid empresaId, Guid usuarioId, CancellationToken ct = default);
}
