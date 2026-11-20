using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Dto.Notificaciones;
using Nextag.Api.Services.Notificaciones.Interface;
using System.Text.Json;

namespace Nextag.Api.Services.Notificaciones.Implementation
{
    public class WhatsAppWebhookProcesadorService
        : IWhatsAppWebhookProcesadorService
    {
        private readonly NextagContext context;

        private readonly ILogger<WhatsAppWebhookProcesadorService>
            logger;

        private const int MinutosBloqueo = 15;

        public WhatsAppWebhookProcesadorService(
            NextagContext context,
            ILogger<WhatsAppWebhookProcesadorService> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        // ==========================================
        // PROCESAR PENDIENTES
        // ==========================================

        public async Task<WhatsAppWebhookProcesamientoDto>
            ProcesarPendientesAsync(
                int cantidad = 20,
                CancellationToken ct = default)
        {
            cantidad = Math.Clamp(cantidad, 1, 100);

            var resultado =
                new WhatsAppWebhookProcesamientoDto
                {
                    FechaInicio = DateTime.UtcNow
                };

            // ======================================
            // RECUPERAR BLOQUEOS EXPIRADOS
            // ======================================

            await RecuperarInterrumpidosAsync(ct);

            var ahora = DateTime.UtcNow;

            // ======================================
            // CONSULTAR CANDIDATOS
            // ======================================

            var candidatos = await context
                .WhatsAppWebhookEventos
                .AsNoTracking()
                .Where(x =>
                    x.Intentos < x.MaxIntentos &&
                    (
                        (
                            x.Estado == "PENDIENTE" &&
                            (
                                x.FechaProximoIntento == null ||
                                x.FechaProximoIntento <= ahora
                            )
                        )
                        ||
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
            // PROCESAR CADA EVENTO
            // ======================================

            foreach (var eventoId in candidatos)
            {
                ct.ThrowIfCancellationRequested();

                var token = Guid.NewGuid();

                var adquirido = await AdquirirAsync(
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
                    var procesado = await ProcesarEventoAsync(
                        eventoId,
                        token,
                        ct);

                    if (procesado)
                    {
                        resultado.Procesados++;
                    }
                    else
                    {
                        resultado.Reprogramados++;
                    }
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
                        "Error procesando webhook {EventoId}",
                        eventoId);

                    context.ChangeTracker.Clear();

                    try
                    {
                        await RegistrarErrorAsync(
                            eventoId,
                            token,
                            ex.Message,
                            ct);
                    }
                    catch (Exception errorRegistro)
                    {
                        logger.LogError(
                            errorRegistro,
                            "No fue posible registrar el error del webhook {EventoId}",
                            eventoId);
                    }
                }
            }

            resultado.FechaFin = DateTime.UtcNow;

            return resultado;
        }

        // ==========================================
        // RECUPERAR INTERRUPCIONES
        // ==========================================

        private async Task RecuperarInterrumpidosAsync(
            CancellationToken ct)
        {
            var ahora = DateTime.UtcNow;

            var limite =
                ahora.AddMinutes(-MinutosBloqueo);

            var actualizados = await context
                .WhatsAppWebhookEventos
                .Where(x =>
                    x.Estado == "PROCESANDO" &&
                    x.FechaBloqueo < limite)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            x => x.Estado,
                            "ERROR")
                        .SetProperty(
                            x => x.FechaBloqueo,
                            (DateTime?)null)
                        .SetProperty(
                            x => x.BloqueoToken,
                            (Guid?)null)
                        .SetProperty(
                            x => x.FechaProximoIntento,
                            x => x.Intentos < x.MaxIntentos
                                ? ahora
                                : (DateTime?)null)
                        .SetProperty(
                            x => x.UltimoError,
                            "Procesamiento interrumpido.")
                        .SetProperty(
                            x => x.FechaActualizacion,
                            ahora),
                    ct);

            if (actualizados > 0)
            {
                logger.LogWarning(
                    "Webhooks recuperados: {Cantidad}",
                    actualizados);
            }
        }

        // ==========================================
        // ADQUIRIR EVENTO
        // ==========================================

        private async Task<bool> AdquirirAsync(
            Guid eventoId,
            Guid token,
            CancellationToken ct)
        {
            var ahora = DateTime.UtcNow;

            var actualizados = await context
                .WhatsAppWebhookEventos
                .Where(x =>
                    x.Id == eventoId &&
                    x.Intentos < x.MaxIntentos &&
                    (
                        (
                            x.Estado == "PENDIENTE" &&
                            (
                                x.FechaProximoIntento == null ||
                                x.FechaProximoIntento <= ahora
                            )
                        )
                        ||
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
                            x => x.FechaActualizacion,
                            ahora),
                    ct);

            return actualizados == 1;
        }

        // ==========================================
        // PROCESAR EVENTO
        // ==========================================

        private async Task<bool> ProcesarEventoAsync(
            Guid eventoId,
            Guid token,
            CancellationToken ct)
        {
            var evento = await context
                .WhatsAppWebhookEventos
                .AsNoTracking()
                .Where(x =>
                    x.Id == eventoId &&
                    x.Estado == "PROCESANDO" &&
                    x.BloqueoToken == token)
                .Select(x => new
                {
                    x.Id,
                    x.IdentificadorProveedor,
                    x.EstadoMeta,
                    x.PayloadJson
                })
                .FirstOrDefaultAsync(ct);

            if (evento == null)
            {
                throw new InvalidOperationException(
                    "El webhook ya no pertenece a esta ejecución.");
            }

            // ======================================
            // BUSCAR ENVIO CORRESPONDIENTE
            // ======================================

            // No suponemos que el Message ID es
            // único en nuestra tabla: comprobamos
            // que exista una sola coincidencia.

            var envios = await context
                .NotificacionEnvios
                .AsNoTracking()
                .Where(x =>
                    x.Canal == "WHATSAPP" &&
                    x.Proveedor == "META" &&
                    x.IdentificadorProveedor ==
                        evento.IdentificadorProveedor)
                .Select(x => new
                {
                    x.Id,
                    x.EmpresaId,
                    x.Estado
                })
                .Take(2)
                .ToListAsync(ct);

            // ======================================
            // AUN NO EXISTE EL ENVIO
            // ======================================

            if (envios.Count == 0)
            {
                await RegistrarErrorAsync(
                    eventoId,
                    token,
                    "AUN_NO_ASOCIADO: El Message ID todavía no existe en NotificacionEnvio.",
                    ct);

                return false;
            }

            if (envios.Count > 1)
            {
                throw new InvalidOperationException(
                    "Existe más de un envío con el mismo identificador de Meta.");
            }

            var envio = envios[0];

            // ======================================
            // MAPEAR ESTADO
            // ======================================

            var estadoNextag = evento.EstadoMeta switch
            {
                "sent" => "ENVIADO",

                "delivered" => "ENTREGADO",

                "read" => "LEIDO",

                "failed" => "FALLIDO",

                _ => throw new InvalidOperationException(
                    "Estado de Meta no reconocido.")
            };

            string? error = null;

            if (estadoNextag == "FALLIDO")
            {
                error = ObtenerError(evento.PayloadJson);
            }

            var ahora = DateTime.UtcNow;

            // ======================================
            // TRANSACCION
            // ======================================

            await using var transaccion =
                await context.Database.BeginTransactionAsync(ct);

            // ======================================
            // VALIDAR QUE CONSERVAMOS EL BLOQUEO
            // ======================================

            var bloqueoVigente = await context
                .WhatsAppWebhookEventos
                .Where(x =>
                    x.Id == eventoId &&
                    x.Estado == "PROCESANDO" &&
                    x.BloqueoToken == token &&
                    x.FechaBloqueo >
                        ahora.AddMinutes(-MinutosBloqueo))
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            x => x.FechaActualizacion,
                            ahora),
                    ct);

            if (bloqueoVigente != 1)
            {
                throw new InvalidOperationException(
                    "El bloqueo del webhook expiró.");
            }

            // ======================================
            // ACTUALIZAR NOTIFICACION
            // ======================================

            // Solo se permiten transiciones
            // hacia estados posteriores.
            //
            // Los eventos repetidos o atrasados
            // no modifican NotificacionEnvio.

            await context.NotificacionEnvios
                .Where(x =>
                    x.Id == envio.Id &&
                    x.EmpresaId == envio.EmpresaId &&
                    x.Canal == "WHATSAPP" &&
                    x.Proveedor == "META" &&
                    x.IdentificadorProveedor ==
                        evento.IdentificadorProveedor &&
                    (
                        (
                            estadoNextag == "ENTREGADO" &&
                            x.Estado == "ENVIADO"
                        )
                        ||
                        (
                            estadoNextag == "LEIDO" &&
                            (
                                x.Estado == "ENVIADO" ||
                                x.Estado == "ENTREGADO"
                            )
                        )
                        ||
                        (
                            estadoNextag == "FALLIDO" &&
                            x.Estado == "ENVIADO"
                        )
                    ))
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            x => x.Estado,
                            estadoNextag)
                        .SetProperty(
                            x => x.UltimoError,
                            error)
                        .SetProperty(
                            x => x.FechaActualizacion,
                            ahora),
                    ct);

            // ======================================
            // MARCAR WEBHOOK COMO PROCESADO
            // ======================================

            var finalizados = await context
                .WhatsAppWebhookEventos
                .Where(x =>
                    x.Id == eventoId &&
                    x.Estado == "PROCESANDO" &&
                    x.BloqueoToken == token)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            x => x.Estado,
                            "PROCESADO")
                        .SetProperty(
                            x => x.EmpresaId,
                            envio.EmpresaId)
                        .SetProperty(
                            x => x.NotificacionEnvioId,
                            envio.Id)
                        .SetProperty(
                            x => x.FechaProcesamiento,
                            ahora)
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
                            x => x.UltimoError,
                            (string?)null)
                        .SetProperty(
                            x => x.FechaActualizacion,
                            ahora),
                    ct);

            if (finalizados != 1)
            {
                throw new InvalidOperationException(
                    "No fue posible finalizar el webhook.");
            }

            await transaccion.CommitAsync(ct);

            return true;
        }

        // ==========================================
        // REGISTRAR ERROR O REINTENTO
        // ==========================================

        private async Task RegistrarErrorAsync(
            Guid eventoId,
            Guid token,
            string mensaje,
            CancellationToken ct)
        {
            var evento = await context
                .WhatsAppWebhookEventos
                .AsNoTracking()
                .Where(x =>
                    x.Id == eventoId &&
                    x.Estado == "PROCESANDO" &&
                    x.BloqueoToken == token)
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

            DateTime? proximoIntento = null;

            if (!agotado)
            {
                var minutos = Math.Min(
                    60,
                    (int)Math.Pow(
                        2,
                        Math.Min(evento.Intentos, 6)));

                proximoIntento =
                    ahora.AddMinutes(minutos);
            }

            if (mensaje.Length > 1000)
            {
                mensaje = mensaje[..1000];
            }

            await context.WhatsAppWebhookEventos
                .Where(x =>
                    x.Id == eventoId &&
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

        // ==========================================
        // OBTENER ERROR ORIGINAL
        // ==========================================

        private static string ObtenerError(
            string payloadJson)
        {
            using var documento =
                JsonDocument.Parse(payloadJson);

            var raiz = documento.RootElement;

            if (!raiz.TryGetProperty(
                "errors",
                out var errores) ||
                errores.ValueKind != JsonValueKind.Array ||
                errores.GetArrayLength() == 0)
            {
                return "WEBHOOK_META: Fallo de entrega.";
            }

            var error = errores[0];

            var codigo = error.TryGetProperty(
                "code",
                out var codigoJson)
                    ? codigoJson.ToString()
                    : "N/D";

            return $"WEBHOOK_META: Fallo de entrega. Código {codigo}.";
        }
    }
}
