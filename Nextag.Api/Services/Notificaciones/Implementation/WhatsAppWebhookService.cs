using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nextag.Api.Data;
using Nextag.Api.Models.Configuration;
using Nextag.Api.Services.Notificaciones.Interface;
using System.Text.Json;

namespace Nextag.Api.Services.Notificaciones.Implementation
{
    public class WhatsAppWebhookService
        : IWhatsAppWebhookService
    {
        private readonly NextagContext context;

        private readonly WhatsAppOptions options;

        private readonly ILogger<WhatsAppWebhookService>
            logger;

        public WhatsAppWebhookService(
            NextagContext context,
            IOptions<WhatsAppOptions> options,
            ILogger<WhatsAppWebhookService> logger)
        {
            this.context = context;
            this.options = options.Value;
            this.logger = logger;
        }

        // ==========================================
        // PROCESAR WEBHOOK
        // ==========================================

        public async Task ProcesarAsync(
            string payload,
            CancellationToken ct = default)
        {
            using var documento =
                JsonDocument.Parse(payload);

            var raiz = documento.RootElement;

            // ======================================
            // VALIDAR TIPO DE OBJETO
            // ======================================

            if (!raiz.TryGetProperty(
                "object",
                out var tipoObjeto))
            {
                return;
            }

            if (tipoObjeto.GetString() !=
                "whatsapp_business_account")
            {
                return;
            }

            // ======================================
            // RECORRER ENTRADAS
            // ======================================

            if (!raiz.TryGetProperty(
                "entry",
                out var entradas) ||
                entradas.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            foreach (var entrada in entradas.EnumerateArray())
            {
                ct.ThrowIfCancellationRequested();

                // ==================================
                // VALIDAR CUENTA WHATSAPP
                // ==================================

                var wabaId = ObtenerTexto(
                    entrada,
                    "id");

                if (wabaId !=
                    options.WhatsAppBusinessAccountId)
                {
                    continue;
                }

                // ==================================
                // RECUPERAR CAMBIOS
                // ==================================

                if (!entrada.TryGetProperty(
                    "changes",
                    out var cambios) ||
                    cambios.ValueKind !=
                        JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var cambio in cambios.EnumerateArray())
                {
                    var campo = ObtenerTexto(
                        cambio,
                        "field");

                    if (campo != "messages")
                    {
                        continue;
                    }

                    if (!cambio.TryGetProperty(
                        "value",
                        out var valor))
                    {
                        continue;
                    }

                    // ==================================
                    // VALIDAR PHONE NUMBER ID
                    // ==================================

                    if (!valor.TryGetProperty(
                        "metadata",
                        out var metadata))
                    {
                        continue;
                    }

                    var phoneNumberId = ObtenerTexto(
                        metadata,
                        "phone_number_id");

                    if (phoneNumberId != options.PhoneNumberId)
                    {
                        continue;
                    }

                    // ==================================
                    // RECUPERAR ESTADOS
                    // ==================================

                    if (!valor.TryGetProperty(
                        "statuses",
                        out var estados) ||
                        estados.ValueKind != JsonValueKind.Array)
                    {
                        // Los mensajes entrantes no
                        // se procesan en esta versión.
                        continue;
                    }

                    foreach (var estado in estados.EnumerateArray())
                    {
                        ct.ThrowIfCancellationRequested();

                        await ProcesarEstadoAsync(
                            estado,
                            ct);
                    }
                }
            }
        }

        // ==========================================
        // PROCESAR ESTADO
        // ==========================================

        private async Task ProcesarEstadoAsync(
            JsonElement estado,
            CancellationToken ct)
        {
            var identificador = ObtenerTexto(
                estado,
                "id");

            var estadoMeta = ObtenerTexto(
                estado,
                "status");

            if (string.IsNullOrWhiteSpace(
                identificador))
            {
                return;
            }

            // ======================================
            // MAPEAR ESTADO META
            // ======================================

            var estadoNextag = estadoMeta switch
            {
                "sent" => "ENVIADO",

                "delivered" => "ENTREGADO",

                "read" => "LEIDO",

                "failed" => "FALLIDO",

                _ => null
            };

            if (estadoNextag == null)
            {
                return;
            }

            // ======================================
            // RECUPERAR ENVIO
            // ======================================

            var envio = await context.NotificacionEnvios
                .AsNoTracking()
                .Where(x =>
                    x.Canal == "WHATSAPP" &&
                    x.Proveedor == "META" &&
                    x.IdentificadorProveedor ==
                        identificador)
                .Select(x => new
                {
                    x.Id,
                    x.Estado
                })
                .FirstOrDefaultAsync(ct);

            if (envio == null)
            {
                logger.LogDebug(
                    "Webhook WhatsApp sin envío asociado.");
                return;
            }

            // ======================================
            // VALIDAR TRANSICION
            // ======================================

            if (!PuedeActualizar(
                envio.Estado,
                estadoNextag))
            {
                return;
            }

            // ======================================
            // OBTENER ERROR
            // ======================================

            string? error = null;

            if (estadoNextag == "FALLIDO")
            {
                error = ObtenerError(estado);
            }

            var ahora = DateTime.UtcNow;

            // ======================================
            // ACTUALIZAR ESTADO
            // ======================================

            // La condición se repite en SQL Server
            // para evitar actualizaciones incorrectas
            // cuando llegan webhooks simultáneos.

            var actualizados = await context
                .NotificacionEnvios
                .Where(x =>
                    x.Id == envio.Id &&
                    x.Canal == "WHATSAPP" &&
                    x.Proveedor == "META" &&
                    x.IdentificadorProveedor ==
                        identificador &&
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

            if (actualizados > 0)
            {
                logger.LogInformation(
                    "Estado WhatsApp actualizado. " +
                    "EnvioId: {EnvioId}. Estado: {Estado}",
                    envio.Id,
                    estadoNextag);
            }
        }

        // ==========================================
        // VALIDAR TRANSICION
        // ==========================================

        private static bool PuedeActualizar(
        string actual,
        string nuevo)
        {
            return nuevo switch
            {
                "ENVIADO" => false,

                "ENTREGADO" =>
                    actual == "ENVIADO",

                "LEIDO" =>
                    actual is "ENVIADO" or "ENTREGADO",

                "FALLIDO" =>
                    actual == "ENVIADO",

                _ => false
            };
        }

        // ==========================================
        // RECUPERAR ERROR META
        // ==========================================

        private static string ObtenerError(
            JsonElement estado)
        {
            if (!estado.TryGetProperty(
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

            var mensaje =
                $"WEBHOOK_META: Fallo de entrega. Código {codigo}.";

            return mensaje.Length > 1000
                ? mensaje[..1000]
                : mensaje;
        }

        // ==========================================
        // OBTENER TEXTO JSON
        // ==========================================

        private static string? ObtenerTexto(
            JsonElement elemento,
            string propiedad)
        {
            if (elemento.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (!elemento.TryGetProperty(
                propiedad,
                out var valor))
            {
                return null;
            }

            return valor.ValueKind == JsonValueKind.String
                ? valor.GetString()
                : null;
        }
    }
}
