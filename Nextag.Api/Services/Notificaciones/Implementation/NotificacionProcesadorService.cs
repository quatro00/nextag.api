using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto.Notificaciones;
using Nextag.Api.Services.Notificaciones.Interface;

namespace Nextag.Api.Services.Notificaciones.Implementation
{
    public class NotificacionProcesadorService
        : INotificacionProcesadorService
    {
        private readonly NextagContext context;

        private readonly INotificacionDestinatarioService
            destinatarioService;

        private readonly ILogger<NotificacionProcesadorService>
            logger;

        private const int MinutosBloqueo = 5;

        public NotificacionProcesadorService(
            NextagContext context,
            INotificacionDestinatarioService destinatarioService,
            ILogger<NotificacionProcesadorService> logger)
        {
            this.context = context;
            this.destinatarioService = destinatarioService;
            this.logger = logger;
        }

        // ==========================================
        // PROCESAR PENDIENTES
        // ==========================================

        public async Task<NotificacionProcesamientoDto>
            ProcesarPendientesAsync(
                int cantidad = 20,
                CancellationToken ct = default)
        {
            cantidad = Math.Clamp(cantidad, 1, 100);

            var resultado = new NotificacionProcesamientoDto
            {
                FechaInicio = DateTime.UtcNow
            };

            var ahora = DateTime.UtcNow;

            var limiteBloqueo =
                ahora.AddMinutes(-MinutosBloqueo);

            // ======================================
            // CONSULTAR CANDIDATOS
            // ======================================

            var candidatos = await context
                .NotificacionEventoPendientes
                .AsNoTracking()
                .Where(x =>
                    (
                        x.Estado == "PENDIENTE" ||
                        x.Estado == "ERROR"
                    ) &&
                    x.Intentos < x.MaxIntentos &&
                    (
                        x.FechaProximoIntento == null ||
                        x.FechaProximoIntento <= ahora
                    ) ||
                    (
                        x.Estado == "PROCESANDO" &&
                        x.Intentos < x.MaxIntentos &&
                        x.FechaBloqueo < limiteBloqueo
                    ))
                .OrderBy(x => x.FechaCreacion)
                .Select(x => x.Id)
                .Take(cantidad)
                .ToListAsync(ct);

            resultado.Encontrados = candidatos.Count;

            // ======================================
            // PROCESAR INDIVIDUALMENTE
            // ======================================

            foreach (var eventoId in candidatos)
            {
                ct.ThrowIfCancellationRequested();

                var token = Guid.NewGuid();

                var adquirido = await AdquirirEventoAsync(
                    eventoId,
                    token,
                    ct);

                if (!adquirido)
                {
                    resultado.Omitidos++;
                    continue;
                }

                try
                {
                    var generadas = await ProcesarEventoAsync(
                        eventoId,
                        token,
                        ct);

                    resultado.Procesados++;

                    resultado.NotificacionesGeneradas +=
                        generadas.Notificaciones;

                    resultado.EnviosGenerados +=
                        generadas.Envios;
                }
                catch (OperationCanceledException)
                    when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    resultado.Fallidos++;

                    logger.LogError(
                        ex,
                        "Error al procesar evento {EventoId}",
                        eventoId);

                    try
                    {
                        await RegistrarErrorAsync(
                            eventoId,
                            token,
                            ex,
                            ct);
                    }
                    catch (Exception errorRegistro)
                    {
                        logger.LogError(
                            errorRegistro,
                            "No fue posible registrar el error del evento {EventoId}",
                            eventoId);
                    }
                }
            }

            resultado.FechaFin = DateTime.UtcNow;

            return resultado;
        }

        // ==========================================
        // ADQUIRIR EVENTO
        // ==========================================

        private async Task<bool> AdquirirEventoAsync(
            Guid eventoId,
            Guid token,
            CancellationToken ct)
        {
            var ahora = DateTime.UtcNow;

            var limiteBloqueo =
                ahora.AddMinutes(-MinutosBloqueo);

            var actualizados = await context
                .NotificacionEventoPendientes
                .Where(x =>
                    x.Id == eventoId &&
                    x.Intentos < x.MaxIntentos &&
                    (
                        (
                            (
                                x.Estado == "PENDIENTE" ||
                                x.Estado == "ERROR"
                            ) &&
                            (
                                x.FechaProximoIntento == null ||
                                x.FechaProximoIntento <= ahora
                            )
                        ) ||
                        (
                            x.Estado == "PROCESANDO" &&
                            x.FechaBloqueo < limiteBloqueo
                        )
                    ))
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            x => x.Estado,
                            "PROCESANDO")
                        .SetProperty(
                            x => x.BloqueoToken,
                            token)
                        .SetProperty(
                            x => x.FechaBloqueo,
                            ahora)
                        .SetProperty(
                            x => x.Intentos,
                            x => x.Intentos + 1)
                        .SetProperty(
                            x => x.FechaActualizacion,
                            ahora),
                    ct);

            return actualizados == 1;
        }

        // ==========================================
        // PROCESAR EVENTO
        // ==========================================

        private async Task<(int Notificaciones, int Envios)>
            ProcesarEventoAsync(
                Guid eventoId,
                Guid token,
                CancellationToken ct)
        {
            var evento = await context
                .NotificacionEventoPendientes
                .AsNoTracking()
                .Include(x => x.NotificacionEvento)
                .FirstOrDefaultAsync(x =>
                    x.Id == eventoId &&
                    x.Estado == "PROCESANDO" &&
                    x.BloqueoToken == token,
                    ct);

            if (evento == null)
            {
                throw new InvalidOperationException(
                    "El evento ya no pertenece a esta ejecución.");
            }

            if (!evento.TicketId.HasValue)
            {
                throw new InvalidOperationException(
                    "El procesador de tickets requiere un TicketId.");
            }

            // ======================================
            // OBTENER TICKET
            // ======================================

            var ticket = await context.Tickets
                .AsNoTracking()
                .Where(x =>
                    x.Id == evento.TicketId.Value &&
                    x.EmpresaId == evento.EmpresaId)
                .Select(x => new
                {
                    x.Id,
                    x.Folio,
                    x.Titulo
                })
                .FirstOrDefaultAsync(ct);

            if (ticket == null)
            {
                throw new InvalidOperationException(
                    "No se encontró el ticket del evento.");
            }

            // ======================================
            // IDENTIFICAR DESTINATARIOS
            // ======================================

            var destinatarios = await destinatarioService
                .ObtenerDestinatariosAsync(
                    eventoId,
                    ct);

            // ======================================
            // GENERAR MENSAJE
            // ======================================

            var titulo = ObtenerTitulo(
                evento.NotificacionEvento.Clave);

            var mensaje = ObtenerMensaje(
                evento.NotificacionEvento.Clave,
                ticket.Folio);

            var ruta = $"/admin/tickets/{ticket.Id}";

            var ahora = DateTime.UtcNow;

            var totalNotificaciones = 0;
            var totalEnvios = 0;

            // ======================================
            // TRANSACCION
            // ======================================

            await using var transaction =
                await context.Database.BeginTransactionAsync(ct);

            // Verificamos el bloqueo nuevamente.
            // Esta actualización condicional se
            // ejecuta dentro de la transacción.

            var bloqueoVigente = await context
                .NotificacionEventoPendientes
                .Where(x =>
                    x.Id == eventoId &&
                    x.Estado == "PROCESANDO" &&
                    x.BloqueoToken == token &&
                    x.FechaBloqueo >
                        DateTime.UtcNow.AddMinutes(-MinutosBloqueo))
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            x => x.FechaActualizacion,
                            ahora),
                    ct);

            if (bloqueoVigente != 1)
            {
                throw new InvalidOperationException(
                    "El bloqueo del evento expiró o fue transferido.");
            }

            // ======================================
            // CREAR NOTIFICACIONES
            // ======================================

            foreach (var destinatario in destinatarios)
            {
                // Comprobar idempotencia.

                var existe = await context.Notificacions
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.EmpresaId == evento.EmpresaId &&
                        x.EventoPendienteId == eventoId &&
                        x.DestinatarioClave ==
                            destinatario.DestinatarioClave,
                        ct);

                if (existe)
                    continue;

                var notificacion = new Notificacion
                {
                    Id = Guid.NewGuid(),

                    EmpresaId = evento.EmpresaId,

                    EventoPendienteId = eventoId,

                    TicketId = ticket.Id,

                    TipoDestinatario =
                        destinatario.TipoDestinatario,

                    DestinatarioClave =
                        destinatario.DestinatarioClave,

                    UsuarioId = destinatario.UsuarioId,

                    DestinatarioNombre =
                        destinatario.Nombre,

                    DestinatarioCorreo =
                        destinatario.Correo,

                    DestinatarioTelefono =
                        destinatario.Telefono,

                    MostrarInterna =
                        destinatario.NotificacionInterna,

                    Titulo = titulo,

                    Mensaje = mensaje,

                    RutaNavegacion =
                        destinatario.TipoDestinatario == "USUARIO"
                            ? ruta
                            : null,

                    FechaLectura = null,

                    FechaCreacion = ahora,

                    UsuarioCreacion =
                        evento.UsuarioCreacion
                };

                context.Notificacions.Add(
                    notificacion);

                totalNotificaciones++;

                // ==================================
                // CORREO
                // ==================================

                if (
                    destinatario.NotificacionCorreo &&
                    !string.IsNullOrWhiteSpace(
                        destinatario.Correo)
                )
                {
                    context.NotificacionEnvios.Add(
                        CrearEnvio(
                            evento.EmpresaId,
                            notificacion.Id,
                            "CORREO",
                            destinatario.Correo!,
                            ahora));

                    totalEnvios++;
                }

                // ==================================
                // WHATSAPP
                // ==================================

                if (
                    destinatario.NotificacionWhatsapp &&
                    !string.IsNullOrWhiteSpace(
                        destinatario.Telefono)
                )
                {
                    context.NotificacionEnvios.Add(
                        CrearEnvio(
                            evento.EmpresaId,
                            notificacion.Id,
                            "WHATSAPP",
                            destinatario.Telefono!,
                            ahora));

                    totalEnvios++;
                }
            }

            // ======================================
            // GUARDAR NOTIFICACIONES Y ENVIOS
            // ======================================

            await context.SaveChangesAsync(ct);

            // ======================================
            // FINALIZAR EVENTO
            // ======================================

            var finalizados = await context
                .NotificacionEventoPendientes
                .Where(x =>
                    x.Id == eventoId &&
                    x.Estado == "PROCESANDO" &&
                    x.BloqueoToken == token)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            x => x.Estado,
                            "COMPLETADO")
                        .SetProperty(
                            x => x.FechaProcesamiento,
                            DateTime.UtcNow)
                        .SetProperty(
                            x => x.FechaBloqueo,
                            (DateTime?)null)
                        .SetProperty(
                            x => x.BloqueoToken,
                            (Guid?)null)
                        .SetProperty(
                            x => x.FechaProximoIntento,
                            (DateTime?)null)
                        .SetProperty(
                            x => x.UltimoError,
                            (string?)null)
                        .SetProperty(
                            x => x.FechaActualizacion,
                            DateTime.UtcNow),
                    ct);

            if (finalizados != 1)
            {
                throw new InvalidOperationException(
                    "No fue posible finalizar el evento.");
            }

            await transaction.CommitAsync(ct);

            return (
                totalNotificaciones,
                totalEnvios
            );
        }

        // ==========================================
        // CREAR ENVIO
        // ==========================================

        private static NotificacionEnvio CrearEnvio(
            Guid empresaId,
            Guid notificacionId,
            string canal,
            string destino,
            DateTime ahora)
        {
            return new NotificacionEnvio
            {
                Id = Guid.NewGuid(),

                EmpresaId = empresaId,

                NotificacionId = notificacionId,

                Canal = canal,

                Destino = destino,

                Estado = "PENDIENTE",

                Intentos = 0,

                MaxIntentos = 5,

                FechaProximoIntento = ahora,

                FechaBloqueo = null,

                BloqueoToken = null,

                FechaUltimoIntento = null,

                FechaEnviado = null,

                FechaCreacion = ahora
            };
        }

        // ==========================================
        // TITULO
        // ==========================================

        private static string ObtenerTitulo(
            string eventoClave)
        {
            return eventoClave switch
            {
                "TICKET_CREADO" =>
                    "Nuevo ticket",

                "TICKET_ASIGNADO" =>
                    "Ticket asignado",

                "TICKET_REASIGNADO" =>
                    "Ticket reasignado",

                "TICKET_ESTADO_CAMBIADO" =>
                    "Cambio de estado",

                "TICKET_COMENTARIO" =>
                    "Nuevo comentario",

                "TICKET_RESUELTO" =>
                    "Ticket resuelto",

                "TICKET_CERRADO" =>
                    "Ticket cerrado",

                "TICKET_CANCELADO" =>
                    "Ticket cancelado",

                _ => "Notificación de Nextag"
            };
        }

        // ==========================================
        // MENSAJE
        // ==========================================

        private static string ObtenerMensaje(
            string eventoClave,
            string folio)
        {
            return eventoClave switch
            {
                "TICKET_CREADO" =>
                    $"Se creó el ticket {folio}.",

                "TICKET_ASIGNADO" =>
                    $"Se realizó una asignación del ticket {folio}.",

                "TICKET_REASIGNADO" =>
                    $"Se reasignó el ticket {folio}.",

                "TICKET_ESTADO_CAMBIADO" =>
                    $"El ticket {folio} cambió de estado.",

                "TICKET_COMENTARIO" =>
                    $"Se agregó un comentario al ticket {folio}.",

                "TICKET_RESUELTO" =>
                    $"El ticket {folio} fue resuelto.",

                "TICKET_CERRADO" =>
                    $"El ticket {folio} fue cerrado.",

                "TICKET_CANCELADO" =>
                    $"El ticket {folio} fue cancelado.",

                _ =>
                    $"Existe una actualización del ticket {folio}."
            };
        }

        // ==========================================
        // REGISTRAR ERROR
        // ==========================================

        private async Task RegistrarErrorAsync(
            Guid eventoId,
            Guid token,
            Exception ex,
            CancellationToken ct)
        {
            var evento = await context
                .NotificacionEventoPendientes
                .AsNoTracking()
                .Where(x =>
                    x.Id == eventoId &&
                    x.BloqueoToken == token &&
                    x.Estado == "PROCESANDO")
                .Select(x => new
                {
                    x.Intentos,
                    x.MaxIntentos
                })
                .FirstOrDefaultAsync(ct);

            if (evento == null)
                return;

            var ahora = DateTime.UtcNow;

            var agotado =
                evento.Intentos >= evento.MaxIntentos;

            // Backoff exponencial, máximo 60 min.

            var minutos = Math.Min(
                60,
                (int)Math.Pow(
                    2,
                    Math.Min(evento.Intentos, 6)));

            var proximoIntento = agotado
                ? (DateTime?)null
                : ahora.AddMinutes(minutos);

            await context.NotificacionEventoPendientes
                .Where(x =>
                    x.Id == eventoId &&
                    x.BloqueoToken == token &&
                    x.Estado == "PROCESANDO")
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            x => x.Estado,
                            "ERROR")
                        .SetProperty(
                            x => x.UltimoError,
                            ex.Message.Length > 1000
                                ? ex.Message.Substring(0, 1000)
                                : ex.Message)
                        .SetProperty(
                            x => x.FechaProximoIntento,
                            proximoIntento)
                        .SetProperty(
                            x => x.FechaBloqueo,
                            (DateTime?)null)
                        .SetProperty(
                            x => x.BloqueoToken,
                            (Guid?)null)
                        .SetProperty(
                            x => x.FechaActualizacion,
                            ahora),
                    ct);
        }
    }
}
