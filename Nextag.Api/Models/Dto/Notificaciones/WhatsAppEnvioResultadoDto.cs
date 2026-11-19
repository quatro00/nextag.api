namespace Nextag.Api.Models.Dto.Notificaciones
{
    public class WhatsAppEnvioResultadoDto
    {
        public bool Aceptado { get; set; }

        public string? IdentificadorProveedor { get; set; }

        public string Proveedor { get; set; } = "META";

        public string? Estado { get; set; }

        public DateTime FechaSolicitudUtc { get; set; }
    }
}
