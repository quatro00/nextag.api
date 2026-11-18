using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Dto.Notificaciones;
using Nextag.Api.Services.Notificaciones.Interface;
using System.Text.Json;

namespace Nextag.Api.Services.Notificaciones.Implementation
{
    public class NotificacionDestinatarioService
        : INotificacionDestinatarioService
    {
        private readonly NextagContext context;

        public NotificacionDestinatarioService(
            NextagContext context)
        {
            this.context = context;
        }

        public async Task<List<NotificacionDestinatarioDto>>
            ObtenerDestinatariosAsync(
                Guid eventoPendienteId,
                CancellationToken ct = default)
        {
            if (eventoPendienteId == Guid.Empty)
                throw new ArgumentException(
                    "El evento no es válido.");

            var evento = await context
                .NotificacionEventoPendientes
                .AsNoTracking()
                .Where(x => x.Id == eventoPendienteId)
                .Select(x => new
                {
                    x.Id,
                    x.EmpresaId,
                    x.TicketId,
                    x.PayloadJson,
                    x.UsuarioCreacion,

                    EventoId = x.NotificacionEventoId,
                    EventoClave = x.NotificacionEvento.Clave
                })
                .FirstOrDefaultAsync(ct);

            if (evento == null)
                throw new InvalidOperationException(
                    "No se encontró el evento.");

            if (!evento.TicketId.HasValue)
                return new List<NotificacionDestinatarioDto>();

            var ticket = await context.Tickets
                .AsNoTracking()
                .Where(x =>
                    x.Id == evento.TicketId.Value &&
                    x.EmpresaId == evento.EmpresaId)
                .FirstOrDefaultAsync(ct);

            if (ticket == null)
                throw new InvalidOperationException(
                    "No se encontró el ticket.");

            var destinatarios =
                new List<NotificacionDestinatarioDto>();

            var metadata = LeerMetadata(evento.PayloadJson);

            // ======================================
            // SOLICITANTE
            // ======================================

            if (DebeNotificarSolicitante(
                evento.EventoClave,
                metadata.VisibleSolicitante))
            {
                var solicitante =
                    await ObtenerSolicitanteAsync(
                        evento.EmpresaId,
                        ticket.Id,
                        ticket.SolicitanteUsuarioId,
                        ticket.SolicitanteNombre,
                        ticket.SolicitanteCorreo,
                        ticket.SolicitanteTelefono,
                        evento.EventoId,
                        ct);

                if (solicitante != null)
                    destinatarios.Add(solicitante);
            }

            // ======================================
            // EQUIPO / TECNICO
            // ======================================

            if (DebeNotificarSoporte(evento.EventoClave))
            {
                var soporte =
                    await ObtenerSoporteAsync(
                        evento.EmpresaId,
                        ticket.Id,
                        evento.EventoId,
                        ct);

                destinatarios.AddRange(soporte);
            }

            // ======================================
            // COMENTARIOS
            // ======================================

            if (evento.EventoClave == "TICKET_COMENTARIO")
            {
                if (!metadata.AutorUsuarioId.HasValue &&
                    !metadata.AutorExterno)
                {
                    throw new InvalidOperationException(
                        "El evento de comentario no identifica a su autor.");
                }

                if (!metadata.VisibleSolicitante.HasValue)
                {
                    throw new InvalidOperationException(
                        "El evento de comentario no indica su visibilidad.");
                }

                if (metadata.AutorUsuarioId.HasValue)
                {
                    destinatarios.RemoveAll(x =>
                        x.UsuarioId ==
                        metadata.AutorUsuarioId.Value);
                }

                if (metadata.AutorExterno)
                {
                    destinatarios.RemoveAll(x =>
                        x.TipoDestinatario == "EXTERNO");
                }
            }

            // ======================================
            // EVITAR DUPLICADOS
            // ======================================

            destinatarios = destinatarios
                .GroupBy(x => x.DestinatarioClave)
                .Select(x => x.First())
                .Where(x =>
                    x.NotificacionInterna ||
                    x.NotificacionCorreo ||
                    x.NotificacionWhatsapp)
                .ToList();

            return destinatarios;
        }

        private static bool DebeNotificarSolicitante(
            string eventoClave,
            bool? visibleSolicitante)
        {
            return eventoClave switch
            {
                "TICKET_CREADO" => true,

                "TICKET_ESTADO_CAMBIADO" => true,

                "TICKET_RESUELTO" => true,

                "TICKET_CERRADO" => true,

                "TICKET_CANCELADO" => true,

                "TICKET_COMENTARIO" =>
                    visibleSolicitante == true,

                _ => false
            };
        }

        private static bool DebeNotificarSoporte(
            string eventoClave)
        {
            return eventoClave switch
            {
                "TICKET_CREADO" => true,

                "TICKET_ASIGNADO" => true,

                "TICKET_REASIGNADO" => true,

                "TICKET_ESTADO_CAMBIADO" => true,

                "TICKET_COMENTARIO" => true,

                "TICKET_RESUELTO" => true,

                "TICKET_CERRADO" => true,

                "TICKET_CANCELADO" => true,

                _ => false
            };
        }

        private async Task<NotificacionDestinatarioDto?>
            ObtenerSolicitanteAsync(
                Guid empresaId,
                Guid ticketId,
                Guid? solicitanteUsuarioId,
                string? solicitanteNombre,
                string? solicitanteCorreo,
                string? solicitanteTelefono,
                int eventoId,
                CancellationToken ct)
        {
            // ======================================
            // SOLICITANTE REGISTRADO
            // ======================================

            if (solicitanteUsuarioId.HasValue)
            {
                var usuario =
                    await ObtenerUsuarioAsync(
                        empresaId,
                        solicitanteUsuarioId.Value,
                        eventoId,
                        ct);

                if (usuario != null)
                    return usuario;
            }

            // ======================================
            // SOLICITANTE EXTERNO
            // ======================================

            if (solicitanteUsuarioId.HasValue)
                return null;

            var preferencia = await context
                .TicketNotificacionPreferencia
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.TicketId == ticketId,
                    ct);

            // Sin preferencias explícitas no se
            // habilitan envíos externos automáticamente.

            if (preferencia == null)
                return null;

            var correo = solicitanteCorreo?.Trim();

            var telefono = solicitanteTelefono?.Trim();

            var recibirCorreo =
                preferencia.RecibirCorreo &&
                !string.IsNullOrWhiteSpace(correo);

            var recibirWhatsapp =
                preferencia.RecibirWhatsapp &&
                preferencia.ConsentimientoWhatsapp &&
                preferencia.FechaConsentimiento.HasValue &&
                !string.IsNullOrWhiteSpace(telefono);

            if (!recibirCorreo && !recibirWhatsapp)
                return null;

            return new NotificacionDestinatarioDto
            {
                EmpresaId = empresaId,

                UsuarioId = null,

                TipoDestinatario = "EXTERNO",

                DestinatarioClave =
                    $"EXTERNO:{ticketId:N}",

                Nombre = solicitanteNombre,

                Correo = correo,

                Telefono = telefono,

                NotificacionInterna = false,

                NotificacionCorreo = recibirCorreo,

                NotificacionWhatsapp = recibirWhatsapp
            };
        }

        private async Task<List<NotificacionDestinatarioDto>>
            ObtenerSoporteAsync(
                Guid empresaId,
                Guid ticketId,
                int eventoId,
                CancellationToken ct)
        {
            var resultado =
                new List<NotificacionDestinatarioDto>();

            // ======================================
            // ASIGNACION VIGENTE
            // ======================================

            var asignacion = await context
                .TicketAsignacions
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.TicketId == ticketId &&
                    x.FechaFin == null)
                .OrderByDescending(x => x.FechaAsignacion)
                .FirstOrDefaultAsync(ct);

            if (asignacion == null)
                return resultado;

            // ======================================
            // TECNICO ESPECIFICO
            // ======================================

            if (asignacion.UsuarioTecnicoId.HasValue)
            {
                var tecnico =
                    await ObtenerUsuarioAsync(
                        empresaId,
                        asignacion.UsuarioTecnicoId.Value,
                        eventoId,
                        ct);

                if (tecnico != null)
                    resultado.Add(tecnico);

                return resultado;
            }

            // ======================================
            // INTEGRANTES DEL EQUIPO
            // ======================================

            var integrantes = await context
                .EquipoSoporteIntegrantes
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.EquipoSoporteId ==
                        asignacion.EquipoSoporteId &&
                    x.Activo == true)
                .Select(x => x.UsuarioId)
                .Distinct()
                .ToListAsync(ct);

            foreach (var usuarioId in integrantes)
            {
                var integrante =
                    await ObtenerUsuarioAsync(
                        empresaId,
                        usuarioId,
                        eventoId,
                        ct);

                if (integrante != null)
                    resultado.Add(integrante);
            }

            return resultado;
        }

        private async Task<NotificacionDestinatarioDto?>
            ObtenerUsuarioAsync(
                Guid empresaId,
                Guid usuarioId,
                int eventoId,
                CancellationToken ct)
        {
            // ======================================
            // VALIDAR MEMBRESIA EN EMPRESA
            // ======================================

            var usuario = await (
                from u in context.Usuarios.AsNoTracking()

                join ue in context.UsuarioEmpresas.AsNoTracking()
                    on u.Id equals ue.UsuarioId

                where
                    u.Id == usuarioId &&
                    u.Activo == true &&
                    ue.EmpresaId == empresaId &&
                    ue.Activo == true

                select new
                {
                    u.Id,
                    u.Nombre,
                    u.Apellidos,
                    u.Email,
                    u.Celular,
                    u.Telefono
                })
                .FirstOrDefaultAsync(ct);

            if (usuario == null)
                return null;

            // ======================================
            // CONSULTAR PREFERENCIAS
            // ======================================

            var preferencias = await context
                .UsuarioNotificacionPreferencia
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.UsuarioId == usuarioId &&
                    x.NotificacionEventoId == eventoId)
                .ToListAsync(ct);

            bool CanalHabilitado(
                string canal,
                bool predeterminado)
            {
                var preferencia = preferencias
                    .FirstOrDefault(x => x.Canal == canal);

                return preferencia?.Habilitado
                    ?? predeterminado;
            }

            // ======================================
            // CANALES
            // ======================================

            var recibirInterna =
                CanalHabilitado("INTERNA", true);

            var recibirCorreo =
                CanalHabilitado("CORREO", true) &&
                !string.IsNullOrWhiteSpace(usuario.Email);

            var preferenciaWhatsapp =
                preferencias.FirstOrDefault(x =>
                    x.Canal == "WHATSAPP");

            var recibirWhatsapp =
                CanalHabilitado("WHATSAPP", false) &&
                preferenciaWhatsapp?.ConsentimientoWhatsapp == true &&
                preferenciaWhatsapp.FechaConsentimiento.HasValue &&
                !string.IsNullOrWhiteSpace(
                    usuario.Celular ?? usuario.Telefono);

            // ======================================
            // RESULTADO
            // ======================================

            return new NotificacionDestinatarioDto
            {
                EmpresaId = empresaId,

                UsuarioId = usuario.Id,

                TipoDestinatario = "USUARIO",

                DestinatarioClave =
                    $"USUARIO:{usuario.Id:N}",

                Nombre =
                    $"{usuario.Nombre} {usuario.Apellidos}".Trim(),

                Correo = usuario.Email,

                Telefono =
                    usuario.Celular ?? usuario.Telefono,

                NotificacionInterna = recibirInterna,

                NotificacionCorreo = recibirCorreo,

                NotificacionWhatsapp = recibirWhatsapp
            };
        }

        private sealed class EventoMetadata
        {
            public Guid? AutorUsuarioId { get; set; }

            public bool AutorExterno { get; set; }

            public bool? VisibleSolicitante { get; set; }
        }

        private static EventoMetadata LeerMetadata(
            string? payloadJson)
        {
            var resultado = new EventoMetadata();

            if (string.IsNullOrWhiteSpace(payloadJson))
                return resultado;

            using var documento =
                JsonDocument.Parse(payloadJson);

            var raiz = documento.RootElement;

            if (raiz.ValueKind != JsonValueKind.Object)
                return resultado;

            if (raiz.TryGetProperty(
                "autorUsuarioId",
                out var autor))
            {
                if (autor.ValueKind == JsonValueKind.String &&
                    Guid.TryParse(
                        autor.GetString(),
                        out var autorId))
                {
                    resultado.AutorUsuarioId = autorId;
                }
            }

            if (raiz.TryGetProperty(
                "autorExterno",
                out var autorExterno))
            {
                if (autorExterno.ValueKind ==
                    JsonValueKind.True)
                {
                    resultado.AutorExterno = true;
                }
            }

            if (raiz.TryGetProperty(
                "visibleSolicitante",
                out var visible))
            {
                if (visible.ValueKind ==
                    JsonValueKind.True)
                {
                    resultado.VisibleSolicitante = true;
                }
                else if (visible.ValueKind ==
                    JsonValueKind.False)
                {
                    resultado.VisibleSolicitante = false;
                }
            }

            return resultado;
        }
    }
}