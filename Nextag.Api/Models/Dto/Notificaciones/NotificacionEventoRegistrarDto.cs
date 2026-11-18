namespace Nextag.Api.Models.Dto.Notificaciones
{
    public class NotificacionEventoRegistrarDto
    {
        public Guid EmpresaId { get; set; }

        public Guid? TicketId { get; set; }

        public string EventoClave { get; set; } = string.Empty;

        public string ClaveIdempotencia { get; set; } = string.Empty;

        public string? PayloadJson { get; set; }

        public Guid? UsuarioCreacion { get; set; }
    }
}
