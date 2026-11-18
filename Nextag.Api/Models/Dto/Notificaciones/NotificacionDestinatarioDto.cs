namespace Nextag.Api.Models.Dto.Notificaciones
{
    public class NotificacionDestinatarioDto
    {
        public Guid EmpresaId { get; set; }

        public Guid? UsuarioId { get; set; }

        public string TipoDestinatario { get; set; }
            = string.Empty;

        public string DestinatarioClave { get; set; }
            = string.Empty;

        public string? Nombre { get; set; }

        public string? Correo { get; set; }

        public string? Telefono { get; set; }

        public bool NotificacionInterna { get; set; }

        public bool NotificacionCorreo { get; set; }

        public bool NotificacionWhatsapp { get; set; }
    }
}
