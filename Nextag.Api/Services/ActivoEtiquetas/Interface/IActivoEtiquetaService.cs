using Nextag.Api.Models.Dto;
using Nextag.Api.Models.Dto.Etiquetas;

namespace Nextag.Api.Services.ActivoEtiquetas.Interface;

public interface IActivoEtiquetaService
{
    // null: activo inexistente dentro del ámbito de empresa.
    Task<List<ActivoEtiquetaHistorialDto>?> GetHistorialAsync(
        Guid activoId, Guid empresaId, CancellationToken ct = default);
    Task<ActivoEtiquetaHistorialDto?> GetVigenteAsync(
        Guid activoId, Guid empresaId, CancellationToken ct = default);
    Task<ServiceResult<ActivoEtiquetaHistorialDto>> VincularAsync(
        Guid activoId, EtiquetaVincularDto dto, Guid empresaId, Guid usuarioId, CancellationToken ct = default);
    Task<ServiceResult<ActivoEtiquetaHistorialDto>> ReemplazarAsync(
        Guid activoId, EtiquetaReemplazarDto dto, Guid empresaId, Guid usuarioId, CancellationToken ct = default);
    Task<ServiceResult<bool>> DesvincularAsync(
        Guid activoId, EtiquetaDesvincularDto dto, Guid empresaId, Guid usuarioId, CancellationToken ct = default);
}
