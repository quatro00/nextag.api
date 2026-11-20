using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nextag.Api.Data;
using Nextag.Api.Models.Configuration;
using Nextag.Api.Models.Domain;
using Nextag.Api.Services.Notificaciones.Interface;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Nextag.Api.Services.Notificaciones.Implementation
{
    public class WhatsAppWebhookService
         : IWhatsAppWebhookService
    {
        private readonly NextagContext context;

        private readonly WhatsAppOptions options;

        private readonly ILogger<WhatsAppWebhookService> logger;

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
        // ALMACENAR WEBHOOK
        // ==========================================

        public async Task<int> AlmacenarAsync(
            string payload,
            CancellationToken ct = default)
        {
            using var documento = JsonDocument.Parse(payload);

            var raiz = documento.RootElement;

            if (ObtenerTexto(raiz, "object") !=
                "whatsapp_business_account")
            {
                return 0;
            }

            if (!raiz.TryGetProperty("entry", out var entradas) ||
                entradas.ValueKind != JsonValueKind.Array)
            {
                return 0;
            }

            var almacenados = 0;

            foreach (var entrada in entradas.EnumerateArray())
            {
                ct.ThrowIfCancellationRequested();

                var wabaId = ObtenerTexto(entrada, "id");

                if (wabaId != options.WhatsAppBusinessAccountId)
                    continue;

                if (!entrada.TryGetProperty(
                    "changes",
                    out var cambios) ||
                    cambios.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var cambio in cambios.EnumerateArray())
                {
                    if (ObtenerTexto(cambio, "field") != "messages")
                        continue;

                    if (!cambio.TryGetProperty(
                        "value",
                        out var valor) ||
                        valor.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    if (!valor.TryGetProperty(
                        "metadata",
                        out var metadata))
                    {
                        continue;
                    }

                    var phoneNumberId =
                        ObtenerTexto(metadata, "phone_number_id");

                    if (phoneNumberId != options.PhoneNumberId)
                        continue;

                    if (!valor.TryGetProperty(
                        "statuses",
                        out var estados) ||
                        estados.ValueKind != JsonValueKind.Array)
                    {
                        // Nextag v1 no procesa mensajes entrantes.
                        continue;
                    }

                    foreach (var estado in estados.EnumerateArray())
                    {
                        ct.ThrowIfCancellationRequested();

                        var agregado = await AlmacenarEstadoAsync(
                            wabaId!,
                            phoneNumberId!,
                            estado,
                            ct);

                        if (agregado)
                            almacenados++;
                    }
                }
            }

            logger.LogInformation(
                "Webhook WhatsApp recibido. Eventos nuevos: {Cantidad}",
                almacenados);

            return almacenados;
        }

        // ==========================================
        // ALMACENAR ESTADO
        // ==========================================

        private async Task<bool> AlmacenarEstadoAsync(
            string wabaId,
            string phoneNumberId,
            JsonElement estado,
            CancellationToken ct)
        {
            var identificador = ObtenerTexto(
                estado,
                "id");

            var estadoMeta = ObtenerTexto(
                estado,
                "status");

            if (string.IsNullOrWhiteSpace(identificador))
                return false;

            if (estadoMeta is not
                ("sent" or "delivered" or "read" or "failed"))
            {
                return false;
            }

            // ======================================
            // FECHA ORIGINAL DE META
            // ======================================

            DateTime? fechaEventoMeta = null;

            var timestamp = ObtenerTexto(
                estado,
                "timestamp");

            if (long.TryParse(timestamp, out var segundos))
            {
                try
                {
                    fechaEventoMeta =
                        DateTimeOffset
                            .FromUnixTimeSeconds(segundos)
                            .UtcDateTime;
                }
                catch (ArgumentOutOfRangeException)
                {
                    // Conservamos fecha nula.
                }
            }

            // ======================================
            // CLAVE DE IDEMPOTENCIA
            // ======================================

            var recipientId = ObtenerTexto(
                estado,
                "recipient_id") ?? string.Empty;

            var contenidoClave = JsonSerializer.Serialize(
                new
                {
                    wabaId,
                    phoneNumberId,
                    identificador,
                    estadoMeta,
                    timestamp,
                    recipientId
                });

            var hash = SHA256.HashData(
                Encoding.UTF8.GetBytes(contenidoClave));

            var claveIdempotencia =
                Convert.ToHexString(hash).ToLowerInvariant();

            // ======================================
            // VALIDAR DUPLICADO EXISTENTE
            // ======================================

            var existe = await context
                .WhatsAppWebhookEventos
                .AsNoTracking()
                .AnyAsync(x =>
                    x.ClaveIdempotencia == claveIdempotencia,
                    ct);

            if (existe)
                return false;

            // ======================================
            // CREAR EVENTO
            // ======================================

            var ahora = DateTime.UtcNow;

            var evento = new WhatsAppWebhookEvento
            {
                Id = Guid.NewGuid(),

                ClaveIdempotencia = claveIdempotencia,

                WhatsAppBusinessAccountId = wabaId,

                PhoneNumberId = phoneNumberId,

                IdentificadorProveedor = identificador,

                EstadoMeta = estadoMeta,

                FechaEventoMeta = fechaEventoMeta,

                EmpresaId = null,

                NotificacionEnvioId = null,

                PayloadJson = estado.GetRawText(),

                Estado = "PENDIENTE",

                Intentos = 0,

                MaxIntentos = 10,

                FechaProximoIntento = ahora,

                FechaBloqueo = null,

                BloqueoToken = null,

                FechaProcesamiento = null,

                UltimoError = null,

                FechaCreacion = ahora
            };

            context.WhatsAppWebhookEventos.Add(evento);

            try
            {
                await context.SaveChangesAsync(ct);

                return true;
            }
            catch (DbUpdateException ex)
                when (EsIndiceDuplicado(ex))
            {
                // Otra ejecución pudo insertar el
                // mismo evento simultáneamente.
                //
                // El índice único garantiza
                // la idempotencia.

                context.Entry(evento).State =
                    EntityState.Detached;

                return false;
            }
        }

        // ==========================================
        // IDENTIFICAR ERROR DE INDICE UNICO
        // ==========================================

        private static bool EsIndiceDuplicado(
            DbUpdateException exception)
        {
            return exception.InnerException is SqlException sql &&
                   sql.Number is 2601 or 2627;
        }

        // ==========================================
        // OBTENER TEXTO
        // ==========================================

        private static string? ObtenerTexto(
            JsonElement elemento,
            string propiedad)
        {
            if (elemento.ValueKind != JsonValueKind.Object)
                return null;

            if (!elemento.TryGetProperty(
                propiedad,
                out var valor))
            {
                return null;
            }

            return valor.ValueKind switch
            {
                JsonValueKind.String => valor.GetString(),

                JsonValueKind.Number => valor.ToString(),

                _ => null
            };
        }
    }
}
