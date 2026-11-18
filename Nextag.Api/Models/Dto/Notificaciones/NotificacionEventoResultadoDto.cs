namespace Nextag.Api.Models.Dto.Notificaciones
{
    public class NotificacionEventoResultadoDto
    {
        public Guid Id { get; set; }

        public bool Creado { get; set; }

        public bool YaExistia => !Creado;

        public string Estado { get; set; } = string.Empty;
    }
}
