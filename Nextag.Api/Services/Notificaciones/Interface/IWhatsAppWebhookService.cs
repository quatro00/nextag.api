namespace Nextag.Api.Services.Notificaciones.Interface
{
    public interface IWhatsAppWebhookService
    {
        Task<int> AlmacenarAsync(
            string payload,
            CancellationToken ct = default);
    }
}
