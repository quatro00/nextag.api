using Nextag.Api.Models.Dto.Notificaciones;

namespace Nextag.Api.Services.Notificaciones.Interface
{
    public interface INotificacionDestinatarioService
    {
        Task<List<NotificacionDestinatarioDto>>
            ObtenerDestinatariosAsync(
                Guid eventoPendienteId,
                CancellationToken ct = default);
    }
}
