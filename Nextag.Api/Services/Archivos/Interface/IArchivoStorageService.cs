namespace Nextag.Api.Services.Archivos.Interface
{
    public interface IArchivoStorageService
    {
        // ==========================================
        // GUARDAR ARCHIVO
        // ==========================================

        Task<string> GuardarAsync(
            Stream contenido,
            Guid empresaId,
            Guid ticketId,
            Guid archivoId,
            string extension,
            CancellationToken ct = default);

        // ==========================================
        // ABRIR ARCHIVO PARA LECTURA
        // ==========================================

        Task<Stream> AbrirLecturaAsync(
            string rutaStorage,
            CancellationToken ct = default);

        // ==========================================
        // ELIMINAR ARCHIVO FISICO
        // ==========================================

        Task EliminarAsync(
            string rutaStorage,
            CancellationToken ct = default);

        // ==========================================
        // VERIFICAR EXISTENCIA
        // ==========================================

        Task<bool> ExisteAsync(
            string rutaStorage,
            CancellationToken ct = default);
    }
}
