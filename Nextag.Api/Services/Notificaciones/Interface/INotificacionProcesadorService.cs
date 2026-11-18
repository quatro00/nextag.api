using Nextag.Api.Models.Dto.Notificaciones;

namespace Nextag.Api.Services.Notificaciones.Interface
{
    public interface INotificacionProcesadorService
    {
        Task<NotificacionProcesamientoDto> ProcesarPendientesAsync(
            int cantidad = 20,
            CancellationToken ct = default);
    }
}
