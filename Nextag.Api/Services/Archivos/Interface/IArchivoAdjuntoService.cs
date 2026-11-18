using Nextag.Api.Models.Dto.Archivos;

namespace Nextag.Api.Services.Archivos.Interface
{
    public interface IArchivoAdjuntoService
    {
        // ==========================================
        // CARGA INTERNA
        // ==========================================

        Task<ArchivoAdjuntoResultadoDto> SubirInternoAsync(
            Guid empresaId,
            Guid ticketId,
            Guid usuarioId,
            bool esAdministrador,
            bool esAdministradorGlobal,
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
        // ==========================================
        // LISTAR ADJUNTOS
        // ==========================================

        Task<List<ArchivoAdjuntoDto>> ListarAsync(
            Guid empresaId,
            Guid ticketId,
            Guid usuarioId,
            bool esAdministrador,
            CancellationToken ct = default);

        // ==========================================
        // DESCARGAR ADJUNTO
        // ==========================================

        Task<ArchivoDescargaDto?> DescargarAsync(
            Guid empresaId,
            Guid ticketId,
            Guid archivoId,
            Guid usuarioId,
            bool esAdministrador,
            CancellationToken ct = default);

        // ==========================================
        // ELIMINAR ADJUNTO
        // ==========================================

        Task<bool> EliminarAsync(
            Guid empresaId,
            Guid ticketId,
            Guid archivoId,
            Guid usuarioId,
            bool esAdministrador,
            CancellationToken ct = default);
    }
}
