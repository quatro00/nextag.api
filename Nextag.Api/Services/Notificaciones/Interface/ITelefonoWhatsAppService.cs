namespace Nextag.Api.Services.Notificaciones.Interface
{
    public interface ITelefonoWhatsAppService
    {
        string Normalizar(
            string telefono,
            string? codigoPais = null);
    }
}
