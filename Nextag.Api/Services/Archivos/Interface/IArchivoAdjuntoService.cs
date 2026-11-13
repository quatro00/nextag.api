using Nextag.Api.Models.Dto.Archivos;

namespace Nextag.Api.Services.Archivos.Interface
{
    public interface IArchivoAdjuntoService
    {
        // ==========================================
        // CARGA INTERNA
        // ==========================================

        Task<ArchivoAdjuntoResultadoDto>
            SubirInternoAsync(
                Guid empresaId,
                Guid ticketId,
                Guid usuarioId,
                bool esAdministrador,
                IFormFile archivo,
                CancellationToken ct = default);

        // ==========================================
        // CARGA PUBLICA
        // ==========================================

        Task<ArchivoAdjuntoResultadoDto>
            SubirPublicoAsync(
                Guid ticketId,
                string token,
                IFormFile archivo,
                CancellationToken ct = default);
    }
}
