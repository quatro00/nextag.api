using Nextag.Api.Models.Dto.Etiquetas.Publicas;

namespace Nextag.Api.Services.EtiquetasPublicas.Interface
{
    public interface IEtiquetaPublicaService
    {
        Task<EtiquetaPublicaDto?> ConsultarAsync(
            string tokenPublico,
            string? tipoLectura,
            string? ip,
            string? userAgent,
            CancellationToken ct = default);
    }
}
