using Nextag.Api.Models.Dto.Notificaciones;

namespace Nextag.Api.Services.Notificaciones.Interface
{
    public interface INotificacionInternaService
    {
        // ==========================================
        // LISTADO
        // ==========================================

        Task<NotificacionPaginadoDto> ObtenerAsync(
            Guid empresaId,
            Guid usuarioId,
            NotificacionConsultaDto consulta,
            CancellationToken ct = default);

        // ==========================================
        // CONTADOR NO LEIDAS
        // ==========================================

        Task<int> ContarNoLeidasAsync(
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);

        // ==========================================
        // MARCAR UNA COMO LEIDA
        // ==========================================

        Task<bool> MarcarLeidaAsync(
            Guid empresaId,
            Guid usuarioId,
            Guid notificacionId,
            CancellationToken ct = default);

        // ==========================================
        // MARCAR TODAS COMO LEIDAS
        // ==========================================

        Task<int> MarcarTodasLeidasAsync(
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);
    }
}
