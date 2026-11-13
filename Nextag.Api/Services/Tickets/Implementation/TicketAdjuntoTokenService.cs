using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Services.Tickets.Interface;
using System.Security.Cryptography;
using System.Text;

namespace Nextag.Api.Services.Tickets.Implementation
{
    public class TicketAdjuntoTokenService
        : ITicketAdjuntoTokenService
    {
        private readonly NextagContext context;

        private const int MinutosVigencia = 15;

        private const int MaximoArchivos = 5;

        public TicketAdjuntoTokenService(
            NextagContext context)
        {
            this.context = context;
        }

        // ==========================================
        // GENERAR TOKEN TEMPORAL
        // ==========================================

        public async Task<(
            string Token,
            DateTime FechaExpiracion
        )> CrearAsync(
            Guid empresaId,
            Guid ticketId,
            CancellationToken ct = default)
        {
            if (
                empresaId == Guid.Empty ||
                ticketId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Los identificadores del ticket no son válidos.");
            }

            // ======================================
            // GENERAR TOKEN ALEATORIO
            // ======================================

            var bytes = RandomNumberGenerator.GetBytes(32);

            var tokenOriginal = Convert
                .ToHexString(bytes)
                .ToLowerInvariant();

            // ======================================
            // GENERAR HASH
            // ======================================

            var tokenHash = CalcularHash(
                tokenOriginal);

            // ======================================
            // VIGENCIA
            // ======================================

            var ahora = DateTime.UtcNow;

            var expiracion = ahora.AddMinutes(
                MinutosVigencia);

            // ======================================
            // CREAR REGISTRO
            // ======================================

            var entidad = new TicketAdjuntoToken
            {
                Id = Guid.NewGuid(),

                EmpresaId = empresaId,

                TicketId = ticketId,

                TokenHash = tokenHash,

                FechaCreacion = ahora,

                FechaExpiracion = expiracion,

                MaximoArchivos = MaximoArchivos,

                ArchivosCargados = 0,

                Revocado = false,

                FechaRevocacion = null,

                UltimaUtilizacion = null
            };

            context.TicketAdjuntoTokens.Add(entidad);

            // IMPORTANTE:
            // No llamamos SaveChangesAsync aquí.
            //
            // El servicio que crea el ticket
            // confirmará ambos registros dentro
            // de la misma transacción.

            await Task.CompletedTask;

            return (
                tokenOriginal,
                expiracion
            );
        }

        // ==========================================
        // VALIDAR TOKEN TEMPORAL
        // ==========================================

        public async Task<TicketAdjuntoToken?>
            ValidarAsync(
                Guid ticketId,
                string token,
                CancellationToken ct = default)
        {
            if (ticketId == Guid.Empty)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            token = token.Trim();

            // ======================================
            // VALIDAR FORMATO
            // ======================================

            if (
                token.Length != 64 ||
                !token.All(Uri.IsHexDigit))
            {
                return null;
            }

            // ======================================
            // CALCULAR HASH
            // ======================================

            var tokenHash = CalcularHash(
                token.ToLowerInvariant());

            var ahora = DateTime.UtcNow;

            // ======================================
            // CONSULTAR TOKEN VIGENTE
            // ======================================

            var autorizacion = await context
                .TicketAdjuntoTokens
                .AsNoTracking()
                .Where(x =>
                    x.TicketId == ticketId &&
                    x.TokenHash == tokenHash &&
                    x.Revocado == false &&
                    x.FechaExpiracion > ahora)
                .FirstOrDefaultAsync(ct);

            if (autorizacion == null)
            {
                return null;
            }

            // ======================================
            // VALIDAR TICKET
            // ======================================

            var ticketValido = await context
                .Tickets
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == autorizacion.TicketId &&
                    x.EmpresaId == autorizacion.EmpresaId &&
                    x.Origen == "PUBLICO" &&
                    x.EstadoTicket.Clave == "ABIERTO" &&
                    x.Empresa.Activo == true,
                    ct);

            if (!ticketValido)
            {
                return null;
            }

            // ======================================
            // VALIDAR CONFIGURACION PUBLICA
            // ======================================

            var configuracionValida = await context
                .EtiquetaConfiguracions
                .AsNoTracking()
                .AnyAsync(x =>
                    x.EmpresaId == autorizacion.EmpresaId &&
                    x.Activo == true &&
                    x.PermitirTicketPublico == true,
                    ct);

            if (!configuracionValida)
            {
                return null;
            }

            // ======================================
            // VALIDAR CUPO
            // ======================================

            if (
                autorizacion.ArchivosCargados >=
                autorizacion.MaximoArchivos)
            {
                return null;
            }

            return autorizacion;
        }

        // ==========================================
        // CALCULAR HASH SHA256
        // ==========================================

        private static string CalcularHash(
            string token)
        {
            var bytes = Encoding.UTF8.GetBytes(
                token);

            var hash = SHA256.HashData(bytes);

            return Convert
                .ToHexString(hash)
                .ToLowerInvariant();
        }
    }
}
