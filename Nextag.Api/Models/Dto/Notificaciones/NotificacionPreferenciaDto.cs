namespace Nextag.Api.Models.Dto.Notificaciones
{
    public class NotificacionPreferenciaDto
    {
        public int EventoId { get; set; }

        public string EventoClave { get; set; }
            = string.Empty;

        public string EventoNombre { get; set; }
            = string.Empty;

        public string? EventoDescripcion { get; set; }

        // ======================================
        // CANALES
        // ======================================

        public bool Interna { get; set; }

        public bool Correo { get; set; }

        public bool Whatsapp { get; set; }

        // ======================================
        // CONSENTIMIENTO WHATSAPP
        // ======================================

        public bool ConsentimientoWhatsapp { get; set; }

        public DateTime? FechaConsentimientoWhatsapp
        {
            get;
            set;
        }

        public string? VersionAviso { get; set; }

        // ======================================
        // DISPONIBILIDAD
        // ======================================

        public bool CorreoDisponible { get; set; }

        public bool WhatsappDisponible { get; set; }
    }
}
