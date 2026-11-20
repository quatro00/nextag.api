namespace Nextag.Api.Models.Dto.Notificaciones
{
    public class WhatsAppWebhookProcesamientoDto
    {
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int Encontrados { get; set; }
        public int Procesados { get; set; }
        public int Actualizados { get; set; }
        public int Reprogramados { get; set; }
        public int Fallidos { get; set; }
        public int Omitidos { get; set; }
    }
}
