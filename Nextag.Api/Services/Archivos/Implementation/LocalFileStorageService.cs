using Microsoft.Extensions.Options;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Configuration;
using Nextag.Api.Services.Archivos.Interface;

namespace Nextag.Api.Services.Archivos.Implementation
{
    public class LocalFileStorageService
        : IArchivoStorageService
    {
        private readonly string rutaBase;

        private readonly long maximoBytesArchivo;

        public LocalFileStorageService(
            IOptions<ArchivoStorageOptions> options,
            IHostEnvironment environment)
        {
            var config = options.Value;

            if (string.IsNullOrWhiteSpace(config.RutaLocal))
            {
                throw new InvalidOperationException(
                    "No está configurada la ruta de almacenamiento.");
            }

            if (config.MaximoBytesArchivo <= 0)
            {
                throw new InvalidOperationException(
                    "El tamaño máximo de archivo no es válido.");
            }

            rutaBase = Path.GetFullPath(
                config.RutaLocal,
                environment.ContentRootPath);

            maximoBytesArchivo =
                config.MaximoBytesArchivo;
        }

        // ==========================================
        // GUARDAR
        // ==========================================

        public async Task<string> GuardarAsync(
    Stream contenido,
    Guid empresaId,
    Guid ticketId,
    Guid archivoId,
    string extension,
    CancellationToken ct = default)
        {
            if (contenido == null || !contenido.CanRead)
            {
                throw new ArgumentException(
                    "El contenido del archivo no es válido.");
            }

            var rutaStorage =
                ArchivoStorageHelper.GenerarRutaTicket(
                    empresaId,
                    ticketId,
                    archivoId,
                    extension);

            var rutaFisica =
                ObtenerRutaFisica(rutaStorage);

            Directory.CreateDirectory(
                Path.GetDirectoryName(rutaFisica)!);

            var archivoCreado = false;

            try
            {
                await using (var destino = new FileStream(
                    rutaFisica,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81920,
                    useAsync: true))
                {
                    archivoCreado = true;

                    var buffer = new byte[81920];

                    long totalBytes = 0;

                    int bytesLeidos;

                    while ((bytesLeidos = await contenido.ReadAsync(
                        buffer.AsMemory(),
                        ct)) > 0)
                    {
                        totalBytes += bytesLeidos;

                        if (totalBytes > maximoBytesArchivo)
                        {
                            throw new InvalidDataException(
                                "El archivo supera el tamaño máximo permitido.");
                        }

                        await destino.WriteAsync(
                            buffer.AsMemory(0, bytesLeidos),
                            ct);
                    }

                    await destino.FlushAsync(ct);
                }

                return rutaStorage;
            }
            catch
            {
                if (archivoCreado && File.Exists(rutaFisica))
                {
                    try
                    {
                        File.Delete(rutaFisica);
                    }
                    catch (IOException)
                    {
                        // Registrar mediante ILogger en una
                        // implementación con logging.
                        // Un proceso de limpieza debe atender
                        // archivos residuales.
                    }
                }

                throw;
            }
        }

        // ==========================================
        // ABRIR LECTURA
        // ==========================================

        public Task<Stream> AbrirLecturaAsync(
            string rutaStorage,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var rutaFisica =
                ObtenerRutaFisica(rutaStorage);

            Stream archivo = new FileStream(
                rutaFisica,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                useAsync: true);

            return Task.FromResult(archivo);
        }

        // ==========================================
        // ELIMINAR
        // ==========================================

        public Task EliminarAsync(
            string rutaStorage,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var rutaFisica =
                ObtenerRutaFisica(rutaStorage);

            if (File.Exists(rutaFisica))
            {
                File.Delete(rutaFisica);
            }

            return Task.CompletedTask;
        }

        // ==========================================
        // EXISTE
        // ==========================================

        public Task<bool> ExisteAsync(
            string rutaStorage,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var rutaFisica =
                ObtenerRutaFisica(rutaStorage);

            return Task.FromResult(
                File.Exists(rutaFisica));
        }

        // ==========================================
        // OBTENER RUTA FISICA
        // ==========================================

        private string ObtenerRutaFisica(
            string rutaStorage)
        {
            rutaStorage =
                ArchivoStorageHelper.ValidarRuta(
                    rutaStorage);

            var rutaRelativa = rutaStorage.Replace(
                '/',
                Path.DirectorySeparatorChar);

            var rutaCompleta = Path.GetFullPath(
                Path.Combine(
                    rutaBase,
                    rutaRelativa));

            var prefijoBase =
                Path.TrimEndingDirectorySeparator(rutaBase)
                + Path.DirectorySeparatorChar;

            if (!rutaCompleta.StartsWith(
                prefijoBase,
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "La ruta está fuera del almacenamiento permitido.");
            }

            return rutaCompleta;
        }
    }
}
