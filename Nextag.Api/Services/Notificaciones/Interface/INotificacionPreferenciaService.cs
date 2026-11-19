using Nextag.Api.Models.Dto.Notificaciones;

namespace Nextag.Api.Services.Notificaciones.Interface
{
    public interface INotificacionPreferenciaService
    {
        Task<List<NotificacionPreferenciaDto>>
            ObtenerAsync(
                Guid empresaId,
                Guid usuarioId,
                CancellationToken ct = default);

        Task<NotificacionPreferenciaDto>
            GuardarAsync(
                Guid empresaId,
                Guid usuarioId,
                int eventoId,
                NotificacionPreferenciaGuardarDto dto,
                CancellationToken ct = default);
    }
}
