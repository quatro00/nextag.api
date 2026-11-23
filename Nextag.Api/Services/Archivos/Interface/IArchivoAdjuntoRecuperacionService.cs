using Nextag.Api.Models.Dto.Archivos;

namespace Nextag.Api.Services.Archivos.Interface
{
    public interface IArchivoAdjuntoRecuperacionService
    {
        // ==========================================
        // PROCESAR ARCHIVOS PENDIENTES
        // ==========================================

        Task<ArchivoRecuperacionResultadoDto>
            ProcesarPendientesAsync(
                int cantidad = 20,
                CancellationToken ct = default);

        // ==========================================
        // LIMPIAR ARCHIVOS RESIDUALES
        // ==========================================

        Task<ArchivoRecuperacionResultadoDto>
            LimpiarResidualesAsync(
                int cantidad = 20,
                CancellationToken ct = default);
    }
}
