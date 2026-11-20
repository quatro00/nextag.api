namespace Nextag.Api.Services.Notificaciones.Interface
{
    public interface IWhatsAppWebhookService
    {
        Task ProcesarAsync(
            string payload,
            CancellationToken ct = default);
    }
}
