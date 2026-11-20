using Nextag.Api.Models.Dto.Notificaciones;

namespace Nextag.Api.Services.Notificaciones.Interface
{
    public interface INotificacionWhatsAppAutorizacionService
    {
        Task<WhatsAppAutorizacionResultadoDto>
            ValidarEnvioAsync(
                Guid envioId,
                Guid empresaId,
                CancellationToken ct = default);
    }
}
