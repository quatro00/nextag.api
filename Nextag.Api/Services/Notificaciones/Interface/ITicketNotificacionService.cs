namespace Nextag.Api.Services.Notificaciones.Interface
{
    public interface ITicketNotificacionService
    {
        Task RegistrarAsync(
            Guid empresaId,
            Guid ticketId,
            string eventoClave,
            Guid operacionId,
            Guid? usuarioId,
            object? payload = null,
            CancellationToken ct = default);
    }
}
