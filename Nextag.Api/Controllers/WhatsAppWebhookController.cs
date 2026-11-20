using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nextag.Api.Models.Configuration;
using Nextag.Api.Services.Notificaciones.Interface;
using System.Security.Cryptography;
using System.Text;

namespace Nextag.Api.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/webhooks/whatsapp")]
    public class WhatsAppWebhookController : ControllerBase
    {
        private readonly WhatsAppWebhookOptions options;

        private readonly IWhatsAppWebhookService webhookService;

        private readonly ILogger<WhatsAppWebhookController>
            logger;

        public WhatsAppWebhookController(
            IOptions<WhatsAppWebhookOptions> options,
            IWhatsAppWebhookService webhookService,
            ILogger<WhatsAppWebhookController> logger)
        {
            this.options = options.Value;
            this.webhookService = webhookService;
            this.logger = logger;
        }

        // ==========================================
        // VERIFICACION INICIAL META
        // ==========================================

        [HttpGet]
        public IActionResult Verificar()
        {
            var mode = Request.Query["hub.mode"].ToString();

            var token = Request.Query["hub.verify_token"]
                .ToString();

            var challenge = Request.Query["hub.challenge"]
                .ToString();

            if (string.IsNullOrWhiteSpace(
                options.VerifyToken))
            {
                return StatusCode(503);
            }

            if (mode != "subscribe" ||
                string.IsNullOrEmpty(challenge))
            {
                return Forbid();
            }

            // ======================================
            // COMPARACION SEGURA
            // ======================================

            var tokenRecibido =
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(token));

            var tokenEsperado =
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(
                        options.VerifyToken));

            if (!CryptographicOperations.FixedTimeEquals(
                tokenRecibido,
                tokenEsperado))
            {
                return StatusCode(403);
            }

            // ======================================
            // RESPONDER DESAFIO META
            // ======================================

            return Content(
                challenge,
                "text/plain");
        }

        // ==========================================
        // RECIBIR WEBHOOK META
        // ==========================================

        [HttpPost]
        [RequestSizeLimit(1024 * 1024)]
        public async Task<IActionResult> Recibir(
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(options.AppSecret))
            {
                logger.LogError(
                    "No se configuró el secreto de la aplicación Meta.");

                return StatusCode(503);
            }

            // ======================================
            // LEER FIRMA META
            // ======================================

            var firma = Request.Headers[
                "X-Hub-Signature-256"].ToString();

            if (string.IsNullOrWhiteSpace(firma) ||
                !firma.StartsWith(
                    "sha256=",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized();
            }

            // ======================================
            // LEER CUERPO ORIGINAL
            // ======================================

            using var memoria = new MemoryStream();

            await Request.Body.CopyToAsync(memoria, ct);

            var bytes = memoria.ToArray();

            // ======================================
            // CALCULAR FIRMA
            // ======================================

            using var hmac = new HMACSHA256(
                Encoding.UTF8.GetBytes(
                    options.AppSecret));

            var firmaCalculada =
                hmac.ComputeHash(bytes);

            byte[] firmaRecibida;

            try
            {
                firmaRecibida = Convert.FromHexString(
                    firma["sha256=".Length..]);
            }
            catch (FormatException)
            {
                return Unauthorized();
            }

            // ======================================
            // VALIDAR FIRMA
            // ======================================

            if (!CryptographicOperations.FixedTimeEquals(
                firmaCalculada,
                firmaRecibida))
            {
                logger.LogWarning(
                    "Webhook WhatsApp rechazado por firma inválida.");

                return Unauthorized();
            }

            // ======================================
            // PROCESAR CONTENIDO
            // ======================================

            var payload = Encoding.UTF8.GetString(bytes);

            try
            {
                await webhookService.ProcesarAsync(
                    payload,
                    ct);
            }
            catch (System.Text.Json.JsonException)
            {
                logger.LogWarning(
                    "Webhook WhatsApp con JSON inválido.");

                return BadRequest();
            }

            // No devolvemos información de los
            // mensajes procesados al proveedor.

            return Ok();
        }
    }
}
