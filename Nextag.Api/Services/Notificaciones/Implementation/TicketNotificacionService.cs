using Nextag.Api.Models.Dto.Notificaciones;
using Nextag.Api.Services.Notificaciones.Interface;
using System.Text.Json;

namespace Nextag.Api.Services.Notificaciones.Implementation
{
    public class TicketNotificacionService
        : ITicketNotificacionService
    {
        private readonly INotificacionEventoService eventoService;

        public TicketNotificacionService(
            INotificacionEventoService eventoService)
        {
            this.eventoService = eventoService;
        }

        // ==========================================
        // REGISTRAR EVENTO DE TICKET
        // ==========================================

        public async Task RegistrarAsync(
            Guid empresaId,
            Guid ticketId,
            string eventoClave,
            Guid operacionId,
            Guid? usuarioId,
            object? payload = null,
            CancellationToken ct = default)
        {
            if (empresaId == Guid.Empty)
            {
                throw new ArgumentException(
                    "La empresa no es válida.");
            }

            if (ticketId == Guid.Empty)
            {
                throw new ArgumentException(
                    "El ticket no es válido.");
            }

            if (operacionId == Guid.Empty)
            {
                throw new ArgumentException(
                    "La operación no es válida.");
            }

            // ======================================
            // CLAVE DE IDEMPOTENCIA
            // ======================================

            var claveIdempotencia =
                $"TICKET:{ticketId:N}:{eventoClave}:{operacionId:N}";

            // ======================================
            // SERIALIZAR PAYLOAD
            // ======================================

            string? payloadJson = null;

            if (payload != null)
            {
                payloadJson = JsonSerializer.Serialize(
                    payload,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy =
                            JsonNamingPolicy.CamelCase
                    });
            }

            // ======================================
            // REGISTRAR EVENTO
            // ======================================

            await eventoService.RegistrarAsync(
                new NotificacionEventoRegistrarDto
                {
                    EmpresaId = empresaId,

                    TicketId = ticketId,

                    EventoClave = eventoClave,

                    ClaveIdempotencia =
                        claveIdempotencia,

                    PayloadJson = payloadJson,

                    UsuarioCreacion = usuarioId
                },
                ct);
        }
    }
}
