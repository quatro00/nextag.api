namespace Nextag.Api.Models.Dto.Notificaciones
{
    public class NotificacionProcesamientoDto
    {
        public int Encontrados { get; set; }
        public int Procesados { get; set; }
        public int Fallidos { get; set; }
        public int Omitidos { get; set; }
        public int NotificacionesGeneradas { get; set; }
        public int EnviosGenerados { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}
