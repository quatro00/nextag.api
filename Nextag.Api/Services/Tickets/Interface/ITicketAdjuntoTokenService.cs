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
    }
}
