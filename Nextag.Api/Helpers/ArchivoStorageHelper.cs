using System.Text.RegularExpressions;

namespace Nextag.Api.Helpers
{
    public static class ArchivoStorageHelper
    {
        private static readonly Regex RutaValida = new(
            @"^empresas/[0-9a-f]{32}/tickets/[0-9a-f]{32}/[0-9a-f]{32}\.(jpg|png|webp|pdf)$",
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant);

        private static readonly HashSet<string>
            ExtensionesPermitidas = new(
                StringComparer.OrdinalIgnoreCase)
            {
                ".jpg",
                ".png",
                ".webp",
                ".pdf"
            };

        // ==========================================
        // GENERAR RUTA
        // ==========================================

        public static string GenerarRutaTicket(
            Guid empresaId,
            Guid ticketId,
            Guid archivoId,
            string extension)
        {
            if (
                empresaId == Guid.Empty ||
                ticketId == Guid.Empty ||
                archivoId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Los identificadores del archivo no son válidos.");
            }

            extension = extension
                .Trim()
                .ToLowerInvariant();

            if (!extension.StartsWith('.'))
            {
                extension = "." + extension;
            }

            if (!ExtensionesPermitidas.Contains(extension))
            {
                throw new ArgumentException(
                    "La extensión del archivo no está permitida.");
            }

            return
                $"empresas/{empresaId:N}/" +
                $"tickets/{ticketId:N}/" +
                $"{archivoId:N}{extension}";
        }

        // ==========================================
        // VALIDAR RUTA INTERNA
        // ==========================================

        public static string ValidarRuta(
            string rutaStorage)
        {
            if (string.IsNullOrWhiteSpace(rutaStorage))
            {
                throw new ArgumentException(
                    "La ruta del archivo es obligatoria.");
            }

            // La ruta interna siempre utiliza /.
            // No aceptamos rutas de Windows ni absolutas.
            if (!RutaValida.IsMatch(rutaStorage))
            {
                throw new ArgumentException(
                    "La ruta del archivo no es válida.");
            }

            return rutaStorage;
        }
    }
}
