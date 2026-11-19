namespace Nextag.Api.Models.Dto.Notificaciones
{
    public class NotificacionPreferenciaGuardarDto
    {
        public bool Interna { get; set; }

        public bool Correo { get; set; }

        public bool Whatsapp { get; set; }

        public bool ConsentimientoWhatsapp { get; set; }

        public string? VersionAviso { get; set; }
    }
}
