using Nextag.Api.Models.Dto.Notificaciones;

namespace Nextag.Api.Services.Notificaciones.Interface
{
    public interface INotificacionEventoService
    {
        // ==========================================
        // REGISTRAR EVENTO
        // ==========================================

        Task<NotificacionEventoResultadoDto> RegistrarAsync(
            NotificacionEventoRegistrarDto dto,
            CancellationToken ct = default);
    }
}
