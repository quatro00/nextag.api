using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit.Utils;
using MimeKit;
using Nextag.Api.Models.Configuration;
using Nextag.Api.Services.Notificaciones.Interface;

namespace Nextag.Api.Services.Notificaciones.Implementation
{
    public class NotificacionSmtpService
        : INotificacionSmtpService
    {
        private readonly SmtpOptions options;

        private readonly ILogger<NotificacionSmtpService> logger;

        public NotificacionSmtpService(
            IOptions<SmtpOptions> options,
            ILogger<NotificacionSmtpService> logger)
        {
            this.options = options.Value;
            this.logger = logger;
        }

        // ==========================================
        // ENVIAR CORREO
        // ==========================================

        public async Task<string> EnviarAsync(
            string destinatario,
            string asunto,
            string cuerpoHtml,
            CancellationToken ct = default)
        {
            // ======================================
            // VALIDAR CONFIGURACION
            // ======================================

            if (string.IsNullOrWhiteSpace(options.Host))
            {
                throw new InvalidOperationException(
                    "No se configuró el servidor SMTP.");
            }

            if (options.Port is < 1 or > 65535)
            {
                throw new InvalidOperationException(
                    "El puerto SMTP no es válido.");
            }

            if (string.IsNullOrWhiteSpace(options.Username) ||
                string.IsNullOrWhiteSpace(options.Password))
            {
                throw new InvalidOperationException(
                    "No se configuraron las credenciales SMTP.");
            }

            if (string.IsNullOrWhiteSpace(options.SenderEmail))
            {
                throw new InvalidOperationException(
                    "No se configuró el remitente SMTP.");
            }

            // ======================================
            // DETERMINAR SEGURIDAD
            // ======================================

            var seguridad = options.Security.ToUpperInvariant() switch
            {
                "STARTTLS" => SecureSocketOptions.StartTls,

                "SSL" => SecureSocketOptions.SslOnConnect,

                _ => throw new InvalidOperationException(
                    "La configuración de seguridad SMTP no es válida.")
            };

            // ======================================
            // CONSTRUIR MENSAJE
            // ======================================

            var mensaje = new MimeMessage();

            mensaje.From.Add(
                new MailboxAddress(
                    options.SenderName,
                    options.SenderEmail));

            mensaje.To.Add(
                MailboxAddress.Parse(destinatario));

            mensaje.Subject = asunto;

            mensaje.MessageId = MimeUtils.GenerateMessageId();

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = cuerpoHtml
            };

            mensaje.Body = bodyBuilder.ToMessageBody();

            // ======================================
            // CONEXION SMTP
            // ======================================

            using var smtp = new SmtpClient();

            smtp.Timeout = 30000;

            try
            {
                await smtp.ConnectAsync(
                    options.Host,
                    options.Port,
                    seguridad,
                    ct);

                // ==================================
                // AUTENTICACION
                // ==================================

                await smtp.AuthenticateAsync(
                    options.Username,
                    options.Password,
                    ct);

                // ==================================
                // ENVIO
                // ==================================

                await smtp.SendAsync(
                    mensaje,
                    ct);

                logger.LogInformation(
                    "Correo aceptado por SMTP. MessageId: {MessageId}",
                    mensaje.MessageId);

                return mensaje.MessageId;
            }
            finally
            {
                if (smtp.IsConnected)
                {
                    try
                    {
                        await smtp.DisconnectAsync(
                            true,
                            CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(
                            ex,
                            "No fue posible cerrar limpiamente la conexión SMTP.");
                    }
                }
            }
        }
    }
}
