namespace Nextag.Api.Models.Configuration
{
    public class WhatsAppWebhookOptions
    {
        public const string SectionName = "WhatsAppWebhook";

        public string VerifyToken { get; set; } =
            string.Empty;

        public string AppSecret { get; set; } =
            string.Empty;
    }
}
