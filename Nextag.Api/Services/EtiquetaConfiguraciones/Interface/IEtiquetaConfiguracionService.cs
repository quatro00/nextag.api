using Nextag.Api.Models.Dto.Etiquetas.Configuracion;

namespace Nextag.Api.Services.EtiquetaConfiguraciones.Interface
{
    public interface IEtiquetaConfiguracionService
    {
        Task<EtiquetaConfiguracionDto> GetAsync(
            Guid empresaId,
            CancellationToken ct = default);

        Task<EtiquetaConfiguracionDto> GuardarAsync(
            Guid empresaId,
            Guid usuarioId,
            EtiquetaConfiguracionGuardarDto dto,
            CancellationToken ct = default);
    }
}
