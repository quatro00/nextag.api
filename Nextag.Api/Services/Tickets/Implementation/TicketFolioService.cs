using Microsoft.Data.SqlClient;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Services.Tickets.Interface;
using Microsoft.EntityFrameworkCore.Storage;

namespace Nextag.Api.Services.Tickets.Implementation
{
    public class TicketFolioService : ITicketFolioService
    {
        private readonly NextagContext context;

        public TicketFolioService(
            NextagContext context)
        {
            this.context = context;
        }

        public async Task<long> SiguienteAsync(
            Guid empresaId,
            CancellationToken ct = default)
        {
            // Debe existir una transacción activa.
            // El llamador confirma la transacción
            // después de guardar el ticket e historial.

            var transaction =
                context.Database.CurrentTransaction
                ?? throw new InvalidOperationException(
                    "Se requiere una transacción activa.");

            var connection =
                context.Database.GetDbConnection();

            await using var command =
                connection.CreateCommand();

            command.Transaction =
                transaction.GetDbTransaction();

            command.CommandText = """
                DECLARE @resultado INT;

                EXEC @resultado = sys.sp_getapplock
                    @Resource = @recurso,
                    @LockMode = 'Exclusive',
                    @LockOwner = 'Transaction',
                    @LockTimeout = 15000;

                SELECT @resultado;
                """;

            command.Parameters.Add(
                new SqlParameter(
                    "@recurso",
                    SqlDbType.NVarChar,
                    255)
                {
                    Value =
                        $"NEXTAG_TICKET_FOLIO_{empresaId:N}"
                });

            var resultado =
                await command.ExecuteScalarAsync(ct);

            var codigo = Convert.ToInt32(resultado);

            if (codigo < 0)
            {
                throw new InvalidOperationException(
                    "No fue posible bloquear el consecutivo de tickets.");
            }

            // ==================================
            // CONTADOR POR EMPRESA
            // ==================================

            var contador =
                await context.TicketFolioContadors
                    .FirstOrDefaultAsync(
                        x => x.EmpresaId == empresaId,
                        ct);

            long numero;

            if (contador == null)
            {
                numero = 1;

                contador = new TicketFolioContador
                {
                    EmpresaId = empresaId,
                    UltimoNumero = numero
                };

                context.TicketFolioContadors.Add(
                    contador);
            }
            else
            {
                numero = checked(
                    contador.UltimoNumero + 1);

                contador.UltimoNumero = numero;
            }

            return numero;
        }
    }
}
