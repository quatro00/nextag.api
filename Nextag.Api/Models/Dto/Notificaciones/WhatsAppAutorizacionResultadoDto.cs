namespace Nextag.Api.Models.Dto.Notificaciones
{
    public class WhatsAppAutorizacionResultadoDto
    {
        public bool Autorizado { get; set; }

        public string? TelefonoNormalizado { get; set; }

        public string? Motivo { get; set; }

        public static WhatsAppAutorizacionResultadoDto
            Permitir(string telefono)
        {
            return new WhatsAppAutorizacionResultadoDto
            {
                Autorizado = true,
                TelefonoNormalizado = telefono
            };
        }

        public static WhatsAppAutorizacionResultadoDto
            Rechazar(string motivo)
        {
            return new WhatsAppAutorizacionResultadoDto
            {
                Autorizado = false,
                Motivo = motivo
            };
        }
    }
}
