namespace Nextag.Api.Models.Configuration
{
    public class WhatsAppOptions
    {
        public const string SectionName = "WhatsApp";

        // ======================================
        // CONFIGURACION GENERAL
        // ======================================

        public bool Enabled { get; set; } = false;

        public string Provider { get; set; } = "Meta";

        public string GraphApiVersion { get; set; } = "v27.0";

        // ======================================
        // IDENTIFICADORES META
        // ======================================

        public string PhoneNumberId { get; set; } =
            string.Empty;

        public string WhatsAppBusinessAccountId { get; set; } =
            string.Empty;

        // ======================================
        // AUTENTICACION
        // ======================================

        public string AccessToken { get; set; } =
            string.Empty;

        // ======================================
        // CONFIGURACION DE MENSAJES
        // ======================================

        public string DefaultLanguage { get; set; } =
            "es_MX";

        public int TimeoutSeconds { get; set; } = 30;

        public int MaxAttempts { get; set; } = 5;
    }
}
