using System.Net;

namespace Nextag.Api.Services.Notificaciones.Implementation
{
    public sealed class MetaWhatsAppException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public int? CodigoMeta { get; }

        public bool PermiteReintento { get; }

        public MetaWhatsAppException(
            HttpStatusCode statusCode,
            int? codigoMeta)
            : base(
                $"Meta rechazó el mensaje. " +
                $"HTTP {(int)statusCode}. " +
                $"Código Meta: {codigoMeta?.ToString() ?? "N/D"}.")
        {
            StatusCode = statusCode;

            CodigoMeta = codigoMeta;

            // ==================================
            // REINTENTOS PERMITIDOS
            // ==================================

            // Solamente reintentamos cuando Meta
            // confirmó un rechazo por límite temporal.

            PermiteReintento =
                statusCode == HttpStatusCode.TooManyRequests ||
                codigoMeta is 130429 or 131056 or 80007;
        }
    }
}
