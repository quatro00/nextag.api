namespace Nextag.Api.Models.Dto.Notificaciones
{
    public class NotificacionConsultaDto
    {
        public int Pagina { get; set; } = 1;

        public int TamanoPagina { get; set; } = 10;

        public bool? SoloNoLeidas { get; set; }
    }
}
