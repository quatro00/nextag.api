namespace Nextag.Api.Models.Dto.Notificaciones
{
    public class NotificacionWhatsAppResultadoDto
    {
        public DateTime FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        public int Encontrados { get; set; }

        public int Procesados { get; set; }

        public int Enviados { get; set; }

        public int Fallidos { get; set; }

        public int Omitidos { get; set; }
    }
}
