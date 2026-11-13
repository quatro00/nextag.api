using Nextag.Api.Models.Domain;

namespace Nextag.Api.Services.Tickets.Interface
{
    public interface ITicketAdjuntoTokenService
    {
        // ==========================================
        // GENERAR TOKEN TEMPORAL
        // ==========================================

        Task<(string Token, DateTime FechaExpiracion)>
            CrearAsync(
                Guid empresaId,
                Guid ticketId,
                CancellationToken ct = default);

        // ==========================================
        // VALIDAR TOKEN TEMPORAL
        // ==========================================

        Task<TicketAdjuntoToken?> ValidarAsync(
            Guid ticketId,
            string token,
            CancellationToken ct = default);

        // Debe ejecutarse dentro de una transacción
        // que también cree el archivo PENDIENTE.
        Task<bool> ReservarAsync(
            Guid autorizacionId,
            CancellationToken ct = default);

        // Debe ejecutarse solamente después de
        // cambiar el archivo PENDIENTE a ERROR.
        Task LiberarReservaAsync(
            Guid autorizacionId,
            CancellationToken ct = default);
    }
}
