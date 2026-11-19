namespace Nextag.Api.Services.Notificaciones.Interface
{
    public interface INotificacionSmtpService
    {
        Task<string> EnviarAsync(
            string destinatario,
            string asunto,
            string cuerpoHtml,
            CancellationToken ct = default);
    }
}
