namespace Nextag.Api.Helpers;

public static class EtiquetaUrlHelper
{
    public static string? Construir(string tokenPublico, IConfiguration configuration)
    {
        var baseUrl = configuration["Etiquetas:UrlPublicaBase"]?.Trim();
        if (string.IsNullOrWhiteSpace(baseUrl)) return null;
        // La URL debe configurarse en el servidor, no la suministra el cliente.
        return $"{baseUrl.TrimEnd('/')}/{tokenPublico}";
    }
}
