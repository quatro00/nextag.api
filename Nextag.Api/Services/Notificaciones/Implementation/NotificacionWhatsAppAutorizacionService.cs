using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Dto.Notificaciones;
using Nextag.Api.Services.Notificaciones.Interface;

namespace Nextag.Api.Services.Notificaciones.Implementation
{
    public class NotificacionWhatsAppAutorizacionService
        : INotificacionWhatsAppAutorizacionService
    {
        private readonly NextagContext context;

        private readonly ITelefonoWhatsAppService
            telefonoService;

        private const string VersionAvisoWhatsapp = "v1";

        public NotificacionWhatsAppAutorizacionService(
            NextagContext context,
            ITelefonoWhatsAppService telefonoService)
        {
            this.context = context;
            this.telefonoService = telefonoService;
        }

        // ==========================================
        // VALIDAR ENVIO
        // ==========================================

        public async Task<WhatsAppAutorizacionResultadoDto>
            ValidarEnvioAsync(
                Guid envioId,
                Guid empresaId,
                CancellationToken ct = default)
        {
            // ======================================
            // OBTENER ENVIO
            // ======================================

            var envio = await context
                .NotificacionEnvios
                .AsNoTracking()
                .Where(x =>
                    x.Id == envioId &&
                    x.EmpresaId == empresaId &&
                    x.Canal == "WHATSAPP")
                .Select(x => new
                {
                    x.Destino,

                    x.Notificacion.TipoDestinatario,

                    x.Notificacion.UsuarioId,

                    x.Notificacion.TicketId,

                    EventoId =
                        x.Notificacion.E.NotificacionEventoId,

                    NotificacionEmpresaId =
                        x.Notificacion.EmpresaId
                })
                .FirstOrDefaultAsync(ct);

            if (envio == null)
            {
                return WhatsAppAutorizacionResultadoDto
                    .Rechazar(
                        "No se encontró el envío WhatsApp.");
            }

            if (envio.NotificacionEmpresaId != empresaId)
            {
                return WhatsAppAutorizacionResultadoDto
                    .Rechazar(
                        "La empresa de la notificación no coincide.");
            }

            // ======================================
            // USUARIO REGISTRADO
            // ======================================

            if (envio.TipoDestinatario == "USUARIO")
            {
                if (!envio.UsuarioId.HasValue)
                {
                    return WhatsAppAutorizacionResultadoDto
                        .Rechazar(
                            "La notificación no tiene usuario.");
                }

                return await ValidarUsuarioAsync(
                    empresaId,
                    envio.UsuarioId.Value,
                    envio.EventoId,
                    envio.Destino,
                    ct);
            }

            // ======================================
            // SOLICITANTE EXTERNO
            // ======================================

            if (envio.TipoDestinatario == "EXTERNO")
            {
                if (!envio.TicketId.HasValue)
                {
                    return WhatsAppAutorizacionResultadoDto
                        .Rechazar(
                            "La notificación no tiene ticket.");
                }

                return await ValidarExternoAsync(
                    empresaId,
                    envio.TicketId.Value,
                    envio.Destino,
                    ct);
            }

            return WhatsAppAutorizacionResultadoDto
                .Rechazar(
                    "El tipo de destinatario no está permitido.");
        }

        // ==========================================
        // VALIDAR USUARIO
        // ==========================================

        private async Task<WhatsAppAutorizacionResultadoDto>
            ValidarUsuarioAsync(
                Guid empresaId,
                Guid usuarioId,
                int eventoId,
                string destino,
                CancellationToken ct)
        {
            // ======================================
            // USUARIO ACTIVO Y MEMBRESIA
            // ======================================

            var usuario = await context.Usuarios
                .AsNoTracking()
                .Where(x =>
                    x.Id == usuarioId &&
                    x.Activo == true &&
                    context.UsuarioEmpresas.Any(ue =>
                        ue.UsuarioId == x.Id &&
                        ue.EmpresaId == empresaId &&
                        ue.Activo == true))
                .Select(x => new
                {
                    x.Celular,
                    x.Telefono,
                    x.CodigoPais
                })
                .FirstOrDefaultAsync(ct);

            if (usuario == null)
            {
                return WhatsAppAutorizacionResultadoDto
                    .Rechazar(
                        "El usuario está inactivo o ya no pertenece a la empresa.");
            }

            // ======================================
            // PREFERENCIA VIGENTE
            // ======================================

            var preferencia = await context
                .UsuarioNotificacionPreferencia
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.UsuarioId == usuarioId &&
                    x.NotificacionEventoId == eventoId &&
                    x.Canal == "WHATSAPP",
                    ct);

            if (
                preferencia == null ||
                preferencia.Habilitado != true ||
                !preferencia.ConsentimientoWhatsapp ||
                !preferencia.FechaConsentimiento.HasValue ||
                preferencia.VersionAviso != VersionAvisoWhatsapp
            )
            {
                return WhatsAppAutorizacionResultadoDto
                    .Rechazar(
                        "El usuario no tiene consentimiento vigente para WhatsApp.");
            }

            // ======================================
            // TELEFONO ACTUAL
            // ======================================

            var telefonoActual =
                !string.IsNullOrWhiteSpace(usuario.Celular)
                    ? usuario.Celular
                    : usuario.Telefono;

            if (string.IsNullOrWhiteSpace(telefonoActual))
            {
                return WhatsAppAutorizacionResultadoDto
                    .Rechazar(
                        "El usuario no tiene teléfono registrado.");
            }

            // ======================================
            // NORMALIZAR Y COMPARAR
            // ======================================

            return ValidarCoincidenciaTelefonos(
                telefonoActual,
                destino,
                usuario.CodigoPais);
        }

        // ==========================================
        // VALIDAR SOLICITANTE EXTERNO
        // ==========================================

        private async Task<WhatsAppAutorizacionResultadoDto>
            ValidarExternoAsync(
                Guid empresaId,
                Guid ticketId,
                string destino,
                CancellationToken ct)
        {
            // ======================================
            // OBTENER TICKET Y PREFERENCIA
            // ======================================

            var ticket = await context.Tickets
                .AsNoTracking()
                .Where(x =>
                    x.Id == ticketId &&
                    x.EmpresaId == empresaId)
                .Select(x => new
                {
                    x.SolicitanteUsuarioId,
                    x.SolicitanteTelefono
                })
                .FirstOrDefaultAsync(ct);

            if (ticket == null)
            {
                return WhatsAppAutorizacionResultadoDto
                    .Rechazar(
                        "No se encontró el ticket.");
            }

            // Evitar tratar a un usuario registrado
            // como destinatario externo.

            if (ticket.SolicitanteUsuarioId.HasValue)
            {
                return WhatsAppAutorizacionResultadoDto
                    .Rechazar(
                        "El solicitante está registrado como usuario.");
            }

            var preferencia = await context
                .TicketNotificacionPreferencia
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.TicketId == ticketId,
                    ct);

            if (
                preferencia == null ||
                !preferencia.RecibirWhatsapp ||
                !preferencia.ConsentimientoWhatsapp ||
                !preferencia.FechaConsentimiento.HasValue ||
                preferencia.VersionAviso != VersionAvisoWhatsapp
            )
            {
                return WhatsAppAutorizacionResultadoDto
                    .Rechazar(
                        "El solicitante no tiene consentimiento vigente para WhatsApp.");
            }

            if (string.IsNullOrWhiteSpace(
                ticket.SolicitanteTelefono))
            {
                return WhatsAppAutorizacionResultadoDto
                    .Rechazar(
                        "El solicitante no tiene teléfono.");
            }

            // En la versión inicial, los tickets
            // públicos utilizan México como país
            // predeterminado.
            //
            // No se debe aplicar esta regla a
            // teléfonos internacionales ambiguos.

            return ValidarCoincidenciaTelefonos(
                ticket.SolicitanteTelefono,
                destino,
                "52");
        }

        // ==========================================
        // VALIDAR COINCIDENCIA DE TELEFONOS
        // ==========================================

        private WhatsAppAutorizacionResultadoDto
            ValidarCoincidenciaTelefonos(
                string telefonoActual,
                string destino,
                string? codigoPais)
        {
            try
            {
                var actualNormalizado =
                    telefonoService.Normalizar(
                        telefonoActual,
                        codigoPais);

                var destinoNormalizado =
                    telefonoService.Normalizar(
                        destino,
                        codigoPais);

                // ==================================
                // COMPARAR
                // ==================================

                if (actualNormalizado != destinoNormalizado)
                {
                    return WhatsAppAutorizacionResultadoDto
                        .Rechazar(
                            "El teléfono del destinatario cambió desde la creación del envío.");
                }

                return WhatsAppAutorizacionResultadoDto
                    .Permitir(actualNormalizado);
            }
            catch (ArgumentException)
            {
                return WhatsAppAutorizacionResultadoDto
                    .Rechazar(
                        "El teléfono no tiene un formato válido.");
            }
        }
    }
}
