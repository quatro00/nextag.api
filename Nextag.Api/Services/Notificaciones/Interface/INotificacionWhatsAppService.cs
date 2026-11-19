using Nextag.Api.Models.Dto.Notificaciones;

namespace Nextag.Api.Services.Notificaciones.Interface
{
    public interface INotificacionWhatsAppService
    {
        Task<WhatsAppEnvioResultadoDto>
            EnviarPlantillaAsync(
                string telefono,
                string plantilla,
                string idioma,
                List<string>? parametros = null,
                CancellationToken ct = default);
    }
}
