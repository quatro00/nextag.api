using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nextag.Api.Data;
using Nextag.Api.Models.Configuration;
using Nextag.Api.Models.Dto.Notificaciones;
using Nextag.Api.Services.Notificaciones.Interface;

namespace Nextag.Api.Services.Notificaciones.Implementation
{
    public class NotificacionWhatsAppProcesadorService
        : INotificacionWhatsAppProcesadorService
    {
        private readonly NextagContext context;

        private readonly INotificacionWhatsAppService
            whatsappService;

        private readonly WhatsAppOptions options;

        private readonly ILogger<NotificacionWhatsAppProcesadorService>
            logger;

        public NotificacionWhatsAppProcesadorService(
            NextagContext context,
            INotificacionWhatsAppService whatsappService,
            IOptions<WhatsAppOptions> options,
            ILogger<NotificacionWhatsAppProcesadorService> logger)
        {
            this.context = context;
            this.whatsappService = whatsappService;
            this.options = options.Value;
            this.logger = logger;
        }

        // ==========================================
        // PROCESAR PENDIENTES
        // ==========================================

        public async Task<NotificacionWhatsAppResultadoDto>
            ProcesarPendientesAsync(
                int cantidad = 20,
                CancellationToken ct = default)
        {
            cantidad = Math.Clamp(cantidad, 1, 100);

            var resultado =
                new NotificacionWhatsAppResultadoDto
                {
                    FechaInicio = DateTime.UtcNow
                };

            // ======================================
            // VALIDAR HABILITACION
            // ======================================

            if (!options.Enabled)
            {
                resultado.FechaFin = DateTime.UtcNow;

                return resultado;
            }

            // ======================================
            // CONSULTAR PENDIENTES
            // ======================================

            var ahora = DateTime.UtcNow;

            var candidatos = await context
                .NotificacionEnvios
                .AsNoTracking()
                .Where(x =>
                    x.Canal == "WHATSAPP" &&
                    x.Estado == "PENDIENTE" &&
                    x.Intentos < x.MaxIntentos &&
                    (
                        x.FechaProximoIntento == null ||
                        x.FechaProximoIntento <= ahora
                    ))
                .OrderBy(x => x.FechaCreacion)
                .Select(x => x.Id)
                .Take(cantidad)
                .ToListAsync(ct);

            resultado.Encontrados = candidatos.Count;

            // ======================================
            // PROCESAR INDIVIDUALMENTE
            // ======================================

            foreach (var envioId in candidatos)
            {
                ct.ThrowIfCancellationRequested();

                var token = Guid.NewGuid();

                var adquirido = await AdquirirAsync(
                    envioId,
                    token,
                    ct);

                if (!adquirido)
                {
                    resultado.Omitidos++;
                    continue;
                }

                resultado.Procesados++;

                try
                {
                    await ProcesarEnvioAsync(
                        envioId,
                        token,
                        ct);

                    resultado.Enviados++;
                }
                catch (OperationCanceledException)
                    when (ct.IsCancellationRequested)
                {
                    // No realizamos un reintento
                    // automático porque Meta podría
                    // haber aceptado el mensaje.

                    throw;
                }
                catch (Exception ex)
                {
                    resultado.Fallidos++;

                    logger.LogError(
                        ex,
                        "Error procesando WhatsApp {EnvioId}",
                        envioId);

                    context.ChangeTracker.Clear();

                    try
                    {
                        await RegistrarErrorAsync(
                            envioId,
                            token,
                            ex,
                            ct);
                    }
                    catch (Exception registroEx)
                    {
                        logger.LogError(
                            registroEx,
                            "No fue posible registrar el error WhatsApp {EnvioId}",
                            envioId);
                    }
                }
            }

            resultado.FechaFin = DateTime.UtcNow;

            return resultado;
        }

        // ==========================================
        // ADQUIRIR ENVIO
        // ==========================================

        private async Task<bool> AdquirirAsync(
            Guid envioId,
            Guid token,
            CancellationToken ct)
        {
            var ahora = DateTime.UtcNow;

            var actualizados = await context
                .NotificacionEnvios
                .Where(x =>
                    x.Id == envioId &&
                    x.Canal == "WHATSAPP" &&
                    x.Estado == "PENDIENTE" &&
                    x.Intentos < x.MaxIntentos &&
                    (
                        x.FechaProximoIntento == null ||
                        x.FechaProximoIntento <= ahora
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
                            x => x.FechaUltimoIntento,
                            ahora)
                        .SetProperty(
                            x => x.FechaActualizacion,
                            ahora),
                    ct);

            return actualizados == 1;
        }

        // ==========================================
        // PROCESAR ENVIO
        // ==========================================

        private async Task ProcesarEnvioAsync(
            Guid envioId,
            Guid token,
            CancellationToken ct)
        {
            // ======================================
            // CONSULTAR NOTIFICACION
            // ======================================

            var envio = await context
                .NotificacionEnvios
                .AsNoTracking()
                .Where(x =>
                    x.Id == envioId &&
                    x.Canal == "WHATSAPP" &&
                    x.Estado == "PROCESANDO" &&
                    x.BloqueoToken == token)
                .Select(x => new
                {
                    x.Id,
                    x.EmpresaId,
                    x.Destino,

                    NotificacionEmpresaId =
                        x.Notificacion.EmpresaId,

                    EventoClave =
                        x.Notificacion.E
                            .NotificacionEvento.Clave,

                    EmpresaNombre =
                        x.Notificacion.Empresa.Nombre,

                    TicketId =
                        x.Notificacion.TicketId
                })
                .FirstOrDefaultAsync(ct);

            if (envio == null)
            {
                throw new InvalidOperationException(
                    "El envío no pertenece a esta ejecución.");
            }

            // ======================================
            // VALIDAR EMPRESA
            // ======================================

            if (envio.EmpresaId !=
                envio.NotificacionEmpresaId)
            {
                throw new InvalidOperationException(
                    "La empresa del envío no coincide con la notificación.");
            }

            if (!envio.TicketId.HasValue)
            {
                throw new InvalidOperationException(
                    "La notificación no tiene un ticket relacionado.");
            }

            // ======================================
            // OBTENER TICKET
            // ======================================

            var ticket = await context.Tickets
                .AsNoTracking()
                .Where(x =>
                    x.Id == envio.TicketId.Value &&
                    x.EmpresaId == envio.EmpresaId)
                .Select(x => new
                {
                    x.Folio,
                    x.Titulo
                })
                .FirstOrDefaultAsync(ct);

            if (ticket == null)
            {
                throw new InvalidOperationException(
                    "No se encontró el ticket relacionado.");
            }

            // ======================================
            // DETERMINAR PLANTILLA
            // ======================================

            var plantilla = ObtenerPlantilla(
                envio.EventoClave);

            // ======================================
            // CONSTRUIR PARAMETROS
            // ======================================

            var parametros = ConstruirParametros(
                envio.EventoClave,
                envio.EmpresaNombre,
                ticket.Folio,
                ticket.Titulo);

            // ======================================
            // VERIFICAR BLOQUEO
            // ======================================

            var bloqueoVigente = await context
                .NotificacionEnvios
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == envioId &&
                    x.Canal == "WHATSAPP" &&
                    x.Estado == "PROCESANDO" &&
                    x.BloqueoToken == token,
                    ct);

            if (!bloqueoVigente)
            {
                throw new InvalidOperationException(
                    "El bloqueo del envío ya no está vigente.");
            }

            // ======================================
            // ENVIAR A META
            // ======================================

            var respuesta = await whatsappService
                .EnviarPlantillaAsync(
                    envio.Destino,
                    plantilla,
                    options.DefaultLanguage,
                    parametros,
                    ct);

            // ======================================
            // VALIDAR RESPUESTA
            // ======================================

            if (!respuesta.Aceptado ||
                string.IsNullOrWhiteSpace(
                    respuesta.IdentificadorProveedor))
            {
                throw new InvalidOperationException(
                    "Meta no confirmó la aceptación del mensaje.");
            }

            // ======================================
            // MARCAR COMO ENVIADO
            // ======================================

            var fechaEnvio = DateTime.UtcNow;

            var actualizados = await context
                .NotificacionEnvios
                .Where(x =>
                    x.Id == envioId &&
                    x.Canal == "WHATSAPP" &&
                    x.Estado == "PROCESANDO" &&
                    x.BloqueoToken == token)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            x => x.Estado,
                            "ENVIADO")
                        .SetProperty(
                            x => x.FechaEnviado,
                            fechaEnvio)
                        .SetProperty(
                            x => x.Proveedor,
                            "META")
                        .SetProperty(
                            x => x.IdentificadorProveedor,
                            respuesta.IdentificadorProveedor)
                        .SetProperty(
                            x => x.UltimoError,
                            (string?)null)
                        .SetProperty(
                            x => x.FechaProximoIntento,
                            (DateTime?)null)
                        .SetProperty(
                            x => x.FechaBloqueo,
                            (DateTime?)null)
                        .SetProperty(
                            x => x.BloqueoToken,
                            (Guid?)null)
                        .SetProperty(
                            x => x.FechaActualizacion,
                            fechaEnvio),
                    ct);

            if (actualizados != 1)
            {
                throw new InvalidOperationException(
                    "Meta aceptó el mensaje, pero no fue posible confirmar el envío en Nextag.");
            }

            logger.LogInformation(
                "WhatsApp aceptado por Meta. EnvioId: {EnvioId}",
                envioId);
        }

        // ==========================================
        // SELECCIONAR PLANTILLA
        // ==========================================

        private static string ObtenerPlantilla(
            string eventoClave)
        {
            return eventoClave switch
            {
                "TICKET_CREADO" =>
                    "nextag_ticket_creado",

                "TICKET_ASIGNADO" or
                "TICKET_REASIGNADO" =>
                    "nextag_ticket_asignado",

                "TICKET_ESTADO_CAMBIADO" or
                "TICKET_COMENTARIO" or
                "TICKET_RESUELTO" or
                "TICKET_CERRADO" or
                "TICKET_CANCELADO" =>
                    "nextag_ticket_actualizacion",

                _ => throw new InvalidOperationException(
                    $"No existe una plantilla WhatsApp para el evento {eventoClave}.")
            };
        }

        // ==========================================
        // CONSTRUIR PARAMETROS
        // ==========================================

        private static List<string> ConstruirParametros(
            string eventoClave,
            string empresa,
            string folio,
            string titulo)
        {
            // ======================================
            // TICKET CREADO
            // ======================================

            if (eventoClave == "TICKET_CREADO")
            {
                return new List<string>
                {
                    empresa,
                    folio,
                    titulo
                };
            }

            // ======================================
            // ASIGNACION
            // ======================================

            if (eventoClave is
                "TICKET_ASIGNADO" or
                "TICKET_REASIGNADO")
            {
                return new List<string>
                {
                    empresa,
                    folio,
                    titulo
                };
            }

            // ======================================
            // ACTUALIZACIONES
            // ======================================

            var descripcion = eventoClave switch
            {
                "TICKET_ESTADO_CAMBIADO" =>
                    "Se actualizó el estado del ticket.",

                "TICKET_COMENTARIO" =>
                    "Se agregó un comentario al seguimiento.",

                "TICKET_RESUELTO" =>
                    "El ticket fue resuelto.",

                "TICKET_CERRADO" =>
                    "El ticket fue cerrado.",

                "TICKET_CANCELADO" =>
                    "El ticket fue cancelado.",

                _ => throw new InvalidOperationException(
                    "El evento no tiene una descripción WhatsApp.")
            };

            return new List<string>
            {
                empresa,
                folio,
                descripcion
            };
        }

        // ==========================================
        // REGISTRAR ERROR
        // ==========================================

        private async Task RegistrarErrorAsync(
            Guid envioId,
            Guid token,
            Exception ex,
            CancellationToken ct)
        {
            var ahora = DateTime.UtcNow;

            var mensaje = ex.Message;

            if (mensaje.Length > 1000)
            {
                mensaje = mensaje[..1000];
            }

            // ======================================
            // NO REINTENTAR AUTOMATICAMENTE
            // ======================================

            // Los reintentos y la clasificación
            // de errores se implementarán en 8L-F.
            //
            // Por ahora evitamos duplicados ante
            // resultados inciertos de Meta.

            await context.NotificacionEnvios
                .Where(x =>
                    x.Id == envioId &&
                    x.Canal == "WHATSAPP" &&
                    x.Estado == "PROCESANDO" &&
                    x.BloqueoToken == token)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            x => x.Estado,
                            "ERROR")
                        .SetProperty(
                            x => x.UltimoError,
                            mensaje)
                        .SetProperty(
                            x => x.FechaProximoIntento,
                            (DateTime?)null)
                        .SetProperty(
                            x => x.Intentos,
                            x => x.MaxIntentos)
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
