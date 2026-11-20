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
        // ==========================================
        // CONFIGURACION DE BLOQUEOS
        // ==========================================

        private const int MinutosBloqueo = 15;

        private readonly NextagContext context;

        private readonly INotificacionWhatsAppService whatsappService;
        private readonly INotificacionWhatsAppAutorizacionService autorizacionService;
        private readonly WhatsAppOptions options;
        private readonly ILogger<NotificacionWhatsAppProcesadorService> logger;

        public NotificacionWhatsAppProcesadorService(
            NextagContext context,
            INotificacionWhatsAppService whatsappService,
             INotificacionWhatsAppAutorizacionService autorizacionService,
            IOptions<WhatsAppOptions> options,
            ILogger<NotificacionWhatsAppProcesadorService> logger)
        {
            this.context = context;
            this.whatsappService = whatsappService;
            this.autorizacionService = autorizacionService;
            this.options = options.Value;
            this.logger = logger;
        }

        // ==========================================
        // RECUPERAR ENVIOS INTERRUMPIDOS
        // ==========================================

        private async Task<int> RecuperarInterrumpidosAsync(
            CancellationToken ct)
        {
            var ahora = DateTime.UtcNow;

            var limiteBloqueo =
                ahora.AddMinutes(-MinutosBloqueo);

            // ======================================
            // IDENTIFICAR BLOQUEOS EXPIRADOS
            // ======================================

            var actualizados = await context
                .NotificacionEnvios
                .Where(x =>
                    x.Canal == "WHATSAPP" &&
                    x.Estado == "PROCESANDO" &&
                    x.FechaBloqueo < limiteBloqueo)
                .ExecuteUpdateAsync(
                    setters => setters

                        // ==================================
                        // ERROR DEFINITIVO
                        // ==================================

                        .SetProperty(
                            x => x.Estado,
                            "ERROR")

                        // ==================================
                        // AGOTAR INTENTOS
                        // ==================================

                        .SetProperty(
                            x => x.Intentos,
                            x => x.MaxIntentos)

                        // ==================================
                        // ELIMINAR PROXIMO INTENTO
                        // ==================================

                        .SetProperty(
                            x => x.FechaProximoIntento,
                            (DateTime?)null)

                        // ==================================
                        // LIBERAR BLOQUEO
                        // ==================================

                        .SetProperty(
                            x => x.FechaBloqueo,
                            (DateTime?)null)

                        .SetProperty(
                            x => x.BloqueoToken,
                            (Guid?)null)

                        // ==================================
                        // REGISTRAR MOTIVO
                        // ==================================

                        .SetProperty(
                            x => x.UltimoError,
                            "RESULTADO_INCIERTO: " +
                            "El procesamiento fue interrumpido. " +
                            "No se reenviará automáticamente.")

                        .SetProperty(
                            x => x.FechaActualizacion,
                            ahora),
                    ct);

            if (actualizados > 0)
            {
                logger.LogWarning(
                    "Se recuperaron {Cantidad} envíos WhatsApp " +
                    "interrumpidos con resultado incierto.",
                    actualizados);
            }

            return actualizados;
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
            // RECUPERAR ENVIOS INTERRUMPIDOS
            // ======================================

            await RecuperarInterrumpidosAsync(ct);

            // ======================================
            // CONSULTAR PENDIENTES
            // ======================================

            var ahora = DateTime.UtcNow;

            var candidatos = await context
                .NotificacionEnvios
                .AsNoTracking()
                .Where(x =>
                    x.Canal == "WHATSAPP" &&

                    x.Intentos < x.MaxIntentos &&

                    (
                        // ==================================
                        // ENVIOS NUEVOS
                        // ==================================

                        (
                            x.Estado == "PENDIENTE" &&

                            (
                                x.FechaProximoIntento == null ||
                                x.FechaProximoIntento <= ahora
                            )
                        )

                        ||

                        // ==================================
                        // REINTENTOS PROGRAMADOS
                        // ==================================

                        (
                            x.Estado == "ERROR" &&

                            x.FechaProximoIntento != null &&

                            x.FechaProximoIntento <= ahora
                        )
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
                    var enviado = await ProcesarEnvioAsync(
                        envioId,
                        token,
                        ct);

                    if (enviado)
                    {
                        resultado.Enviados++;
                    }
                    else
                    {
                        resultado.Omitidos++;
                    }
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

                    x.Intentos < x.MaxIntentos &&

                    (
                        // ==================================
                        // PENDIENTE
                        // ==================================

                        (
                            x.Estado == "PENDIENTE" &&

                            (
                                x.FechaProximoIntento == null ||
                                x.FechaProximoIntento <= ahora
                            )
                        )

                        ||

                        // ==================================
                        // ERROR CON REINTENTO
                        // ==================================

                        (
                            x.Estado == "ERROR" &&

                            x.FechaProximoIntento != null &&

                            x.FechaProximoIntento <= ahora
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

        private async Task<bool> ProcesarEnvioAsync(
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

            var ahora = DateTime.UtcNow;

            var bloqueoVigente = await context
                .NotificacionEnvios
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == envioId &&
                    x.Canal == "WHATSAPP" &&
                    x.Estado == "PROCESANDO" &&
                    x.BloqueoToken == token &&
                    x.FechaBloqueo >
                        ahora.AddMinutes(-MinutosBloqueo),
                    ct);

            if (!bloqueoVigente)
            {
                throw new InvalidOperationException(
                    "El bloqueo del envío expiró.");
            }

            // ======================================
            // VALIDAR AUTORIZACION VIGENTE
            // ======================================

            var autorizacion = await autorizacionService
                .ValidarEnvioAsync(
                    envioId,
                    envio.EmpresaId,
                    ct);

            // ======================================
            // OMITIR ENVIO NO AUTORIZADO
            // ======================================

            if (!autorizacion.Autorizado)
            {
                await OmitirEnvioAsync(
                envioId,
                token,
                autorizacion.Motivo ??
                    "El destinatario no está autorizado.",
                ct);

                return false;
            }

            // ======================================
            // ENVIAR A META
            // ======================================

            var telefonoDestino =
                "+" + autorizacion.TelefonoNormalizado!;

            var respuesta = await whatsappService
                .EnviarPlantillaAsync(
                    telefonoDestino,
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

            return true;
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
        // OMITIR ENVIO NO AUTORIZADO
        // ==========================================

        private async Task OmitirEnvioAsync(
            Guid envioId,
            Guid token,
            string motivo,
            CancellationToken ct)
        {
            var ahora = DateTime.UtcNow;

            var mensaje = $"OMITIDO: {motivo}";

            if (mensaje.Length > 1000)
            {
                mensaje = mensaje[..1000];
            }

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
                            "ERROR")
                        .SetProperty(
                            x => x.Intentos,
                            x => x.MaxIntentos)
                        .SetProperty(
                            x => x.UltimoError,
                            mensaje)
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
                            ahora),
                    ct);

            if (actualizados != 1)
            {
                throw new InvalidOperationException(
                    "No fue posible registrar la omisión WhatsApp.");
            }

            logger.LogInformation(
                "Envío WhatsApp {EnvioId} omitido. Motivo: {Motivo}",
                envioId,
                motivo);
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
            // ======================================
            // CONSULTAR ENVIO
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
                    x.Intentos,
                    x.MaxIntentos
                })
                .FirstOrDefaultAsync(ct);

            if (envio == null)
            {
                return;
            }

            var ahora = DateTime.UtcNow;

            // ======================================
            // CLASIFICAR ERROR
            // ======================================

            // Únicamente se permite reintentar cuando
            // Meta confirmó el rechazo por límite
            // temporal de envío.

            var permiteReintento =
                ex is MetaWhatsAppException metaException &&
                metaException.PermiteReintento;

            // ======================================
            // VALIDAR INTENTOS
            // ======================================

            var agotado =
                envio.Intentos >= envio.MaxIntentos;

            var reintentar =
                permiteReintento && !agotado;

            // ======================================
            // BACKOFF EXPONENCIAL
            // ======================================

            DateTime? proximoIntento = null;

            if (reintentar)
            {
                var minutos = Math.Min(
                    60,
                    (int)Math.Pow(
                        2,
                        Math.Min(envio.Intentos, 6)));

                proximoIntento =
                    ahora.AddMinutes(minutos);
            }

            // ======================================
            // DETERMINAR INTENTOS FINALES
            // ======================================

            // Los errores definitivos o inciertos
            // agotan los intentos para impedir
            // reenvíos automáticos.

            var intentosFinales = reintentar
                ? envio.Intentos
                : envio.MaxIntentos;

            // ======================================
            // CONSTRUIR ERROR
            // ======================================

            string prefijo;

            if (reintentar)
            {
                prefijo = "REINTENTO_PROGRAMADO";
            }
            else if (permiteReintento && agotado)
            {
                prefijo = "INTENTOS_AGOTADOS";
            }
            else if (ex is MetaWhatsAppException)
            {
                prefijo = "RECHAZADO_META";
            }
            else
            {
                prefijo = "RESULTADO_INCIERTO";
            }

            var mensaje =
                $"{prefijo}: {ex.Message}";

            if (mensaje.Length > 1000)
            {
                mensaje = mensaje[..1000];
            }

            // ======================================
            // ACTUALIZAR ENVIO
            // ======================================

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
                            x => x.Intentos,
                            intentosFinales)

                        .SetProperty(
                            x => x.FechaProximoIntento,
                            proximoIntento)

                        .SetProperty(
                            x => x.UltimoError,
                            mensaje)

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

            // ======================================
            // LOG
            // ======================================

            if (reintentar)
            {
                logger.LogWarning(
                    "WhatsApp {EnvioId} tendrá otro intento " +
                    "en {FechaProximoIntento}.",
                    envioId,
                    proximoIntento);
            }
            else
            {
                logger.LogWarning(
                    "WhatsApp {EnvioId} terminó sin " +
                    "reintentos automáticos. Motivo: {Motivo}",
                    envioId,
                    prefijo);
            }
        }
    }
}
