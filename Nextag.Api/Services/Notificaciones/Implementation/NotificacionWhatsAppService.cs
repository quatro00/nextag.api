using Microsoft.Extensions.Options;
using Nextag.Api.Models.Configuration;
using Nextag.Api.Models.Dto.Notificaciones;
using Nextag.Api.Services.Notificaciones.Interface;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Nextag.Api.Services.Notificaciones.Implementation
{
    public class NotificacionWhatsAppService
        : INotificacionWhatsAppService
    {
        private readonly HttpClient httpClient;

        private readonly WhatsAppOptions options;

        private readonly ILogger<NotificacionWhatsAppService> logger;
        private readonly ITelefonoWhatsAppService telefonoService;

        public NotificacionWhatsAppService(
            HttpClient httpClient,
            IOptions<WhatsAppOptions> options,
            ITelefonoWhatsAppService telefonoService,
            ILogger<NotificacionWhatsAppService> logger)
        {
            this.httpClient = httpClient;

            this.options = options.Value;
            this.telefonoService = telefonoService;
            this.logger = logger;
        }

        // ==========================================
        // ENVIAR PLANTILLA
        // ==========================================

        public async Task<WhatsAppEnvioResultadoDto>
            EnviarPlantillaAsync(
                string telefono,
                string plantilla,
                string idioma,
                List<string>? parametros = null,
                CancellationToken ct = default)
        {
            // ======================================
            // VALIDAR CONFIGURACION
            // ======================================

            ValidarConfiguracion();

            // ======================================
            // VALIDAR DESTINATARIO
            // ======================================

            var numeroDestino = telefonoService.Normalizar(telefono);

            // ======================================
            // VALIDAR PLANTILLA
            // ======================================

            if (string.IsNullOrWhiteSpace(plantilla))
            {
                throw new ArgumentException(
                    "El nombre de la plantilla es obligatorio.",
                    nameof(plantilla));
            }

            plantilla = plantilla.Trim();

            if (!Regex.IsMatch(
                plantilla,
                @"^[a-z0-9_]+$"))
            {
                throw new ArgumentException(
                    "El nombre de la plantilla no es válido.",
                    nameof(plantilla));
            }

            // ======================================
            // VALIDAR IDIOMA
            // ======================================

            idioma = string.IsNullOrWhiteSpace(idioma)
                ? options.DefaultLanguage
                : idioma.Trim();

            if (!Regex.IsMatch(
                idioma,
                @"^[a-z]{2}(_[A-Z]{2})?$"))
            {
                throw new ArgumentException(
                    "El idioma de la plantilla no es válido.",
                    nameof(idioma));
            }

            // ======================================
            // CONSTRUIR PARAMETROS
            // ======================================

            var componentes = new List<object>();

            if (parametros != null &&
                parametros.Count > 0)
            {
                var parametrosCuerpo = parametros
                    .Select(valor => new
                    {
                        type = "text",
                        text = valor
                    })
                    .ToList();

                componentes.Add(new
                {
                    type = "body",
                    parameters = parametrosCuerpo
                });
            }

            // ======================================
            // CONSTRUIR SOLICITUD
            // ======================================

            var payload = new
            {
                messaging_product = "whatsapp",

                recipient_type = "individual",

                to = numeroDestino,

                type = "template",

                template = new
                {
                    name = plantilla,

                    language = new
                    {
                        code = idioma
                    },

                    components = componentes
                }
            };

            // ======================================
            // ENDPOINT META
            // ======================================

            var url =
                $"https://graph.facebook.com/" +
                $"{options.GraphApiVersion}/" +
                $"{options.PhoneNumberId}/messages";

            // ======================================
            // HTTP REQUEST
            // ======================================

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    url);

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    options.AccessToken);

            request.Content =
                JsonContent.Create(payload);

            // ======================================
            // ENVIAR
            // ======================================

            using var response =
                await httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    ct);

            // ======================================
            // LEER RESPUESTA
            // ======================================

            var contenido =
                await response.Content
                    .ReadAsStringAsync(ct);

            // ======================================
            // VALIDAR RESPUESTA HTTP
            // ======================================

            if (!response.IsSuccessStatusCode)
            {
                // ==================================
                // OBTENER CODIGO DE ERROR META
                // ==================================

                int? codigoMeta = null;

                try
                {
                    using var errorDocumento =
                        JsonDocument.Parse(contenido);

                    var errorRaiz =
                        errorDocumento.RootElement;

                    if (
                        errorRaiz.TryGetProperty(
                            "error",
                            out var errorMeta) &&
                        errorMeta.ValueKind == JsonValueKind.Object &&
                        errorMeta.TryGetProperty(
                            "code",
                            out var codigo) &&
                        codigo.ValueKind == JsonValueKind.Number &&
                        codigo.TryGetInt32(out var valorCodigo)
                    )
                    {
                        codigoMeta = valorCodigo;
                    }
                }
                catch (JsonException)
                {
                    // Una respuesta de error puede no
                    // contener JSON válido.
                    //
                    // Conservamos el código HTTP.
                }

                // ==================================
                // REGISTRAR ERROR
                // ==================================

                logger.LogWarning(
                    "Meta rechazó WhatsApp. " +
                    "HTTP: {StatusCode}. " +
                    "Código Meta: {CodigoMeta}",
                    (int)response.StatusCode,
                    codigoMeta);

                // ==================================
                // GENERAR EXCEPCION CONTROLADA
                // ==================================

                throw new MetaWhatsAppException(
                    response.StatusCode,
                    codigoMeta);
            }

            // ======================================
            // DESERIALIZAR RESPUESTA
            // ======================================

            using var documento =
                JsonDocument.Parse(contenido);

            var raiz =
                documento.RootElement;

            // ======================================
            // RECUPERAR MESSAGE ID
            // ======================================

            string? identificadorProveedor = null;

            if (
                raiz.TryGetProperty(
                    "messages",
                    out var messages) &&
                messages.ValueKind ==
                    JsonValueKind.Array &&
                messages.GetArrayLength() > 0
            )
            {
                var mensaje =
                    messages[0];

                if (
                    mensaje.TryGetProperty(
                        "id",
                        out var messageId)
                )
                {
                    identificadorProveedor =
                        messageId.GetString();
                }
            }

            // ======================================
            // VALIDAR IDENTIFICADOR
            // ======================================

            if (string.IsNullOrWhiteSpace(
                identificadorProveedor))
            {
                throw new InvalidOperationException(
                    "Meta aceptó la solicitud, pero no devolvió " +
                    "un identificador de mensaje.");
            }

            // ======================================
            // REGISTRAR RESULTADO
            // ======================================

            logger.LogInformation(
                "WhatsApp aceptado por Meta. " +
                "MessageId: {MessageId}",
                identificadorProveedor);

            return new WhatsAppEnvioResultadoDto
            {
                Aceptado = true,

                IdentificadorProveedor =
                    identificadorProveedor,

                Proveedor = "META",

                Estado = "ACEPTADO_META",

                FechaSolicitudUtc =
                    DateTime.UtcNow
            };
        }

        // ==========================================
        // VALIDAR CONFIGURACION
        // ==========================================

        private void ValidarConfiguracion()
        {
            if (!options.Enabled)
            {
                throw new InvalidOperationException(
                    "Las notificaciones WhatsApp " +
                    "se encuentran deshabilitadas.");
            }

            if (options.Provider != "Meta")
            {
                throw new InvalidOperationException(
                    "El proveedor WhatsApp no es válido.");
            }

            if (string.IsNullOrWhiteSpace(
                options.AccessToken))
            {
                throw new InvalidOperationException(
                    "No se configuró el Access Token de Meta.");
            }

            if (string.IsNullOrWhiteSpace(
                options.PhoneNumberId))
            {
                throw new InvalidOperationException(
                    "No se configuró el Phone Number ID.");
            }

            if (!Regex.IsMatch(
                options.PhoneNumberId,
                @"^[0-9]+$"))
            {
                throw new InvalidOperationException(
                    "El Phone Number ID no es válido.");
            }

            if (!Regex.IsMatch(
                options.GraphApiVersion,
                @"^v[0-9]+\.[0-9]+$"))
            {
                throw new InvalidOperationException(
                    "La versión de Graph API no es válida.");
            }
        }
    }
}
