namespace Nextag.Api.Models.Dto.Notificaciones
{
    public class NotificacionCorreoResultadoDto
    {
        public int Encontrados { get; set; }
        public int Enviados { get; set; }
        public int Fallidos { get; set; }
        public int Omitidos { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}
