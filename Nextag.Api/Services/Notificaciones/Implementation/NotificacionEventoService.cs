using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto.Notificaciones;
using Nextag.Api.Services.Notificaciones.Interface;
using System.Text.Json;

namespace Nextag.Api.Services.Notificaciones.Implementation
{
    public class NotificacionEventoService
        : INotificacionEventoService
    {
        private readonly NextagContext context;

        private readonly ILogger<NotificacionEventoService> logger;

        public NotificacionEventoService(
            NextagContext context,
            ILogger<NotificacionEventoService> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        // ==========================================
        // REGISTRAR EVENTO
        // ==========================================

        public async Task<NotificacionEventoResultadoDto> RegistrarAsync(
            NotificacionEventoRegistrarDto dto,
            CancellationToken ct = default)
        {
            // ======================================
            // VALIDACIONES INICIALES
            // ======================================

            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            if (dto.EmpresaId == Guid.Empty)
            {
                throw new ArgumentException(
                    "El identificador de empresa no es válido.");
            }

            if (dto.TicketId == Guid.Empty)
            {
                throw new ArgumentException(
                    "El identificador del ticket no es válido.");
            }

            if (dto.UsuarioCreacion == Guid.Empty)
            {
                throw new ArgumentException(
                    "El identificador del usuario no es válido.");
            }

            var eventoClave = dto.EventoClave?
                .Trim()
                .ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(eventoClave))
            {
                throw new ArgumentException(
                    "Debes proporcionar la clave del evento.");
            }

            if (eventoClave.Length > 60)
            {
                throw new ArgumentException(
                    "La clave del evento supera 60 caracteres.");
            }

            var claveIdempotencia =
                dto.ClaveIdempotencia?.Trim();

            if (string.IsNullOrWhiteSpace(claveIdempotencia))
            {
                throw new ArgumentException(
                    "Debes proporcionar la clave de idempotencia.");
            }

            if (claveIdempotencia.Length > 150)
            {
                throw new ArgumentException(
                    "La clave de idempotencia supera 150 caracteres.");
            }

            // ======================================
            // VALIDAR PAYLOAD JSON
            // ======================================

            string? payloadJson = dto.PayloadJson;

            if (!string.IsNullOrWhiteSpace(payloadJson))
            {
                try
                {
                    using var documento =
                        JsonDocument.Parse(payloadJson);

                    // Aceptamos únicamente objetos JSON.

                    if (documento.RootElement.ValueKind
                        != JsonValueKind.Object)
                    {
                        throw new ArgumentException(
                            "El payload debe ser un objeto JSON.");
                    }
                }
                catch (JsonException ex)
                {
                    throw new ArgumentException(
                        "El payload contiene un JSON inválido.",
                        nameof(dto),
                        ex);
                }
            }
            else
            {
                payloadJson = null;
            }

            // ======================================
            // VALIDAR EMPRESA
            // ======================================

            var empresaExiste = await context.Empresas
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == dto.EmpresaId &&
                    x.Activo == true,
                    ct);

            if (!empresaExiste)
            {
                throw new InvalidOperationException(
                    "La empresa no existe o está inactiva.");
            }

            // ======================================
            // VALIDAR EVENTO
            // ======================================

            var evento = await context.NotificacionEventos
                .AsNoTracking()
                .Where(x =>
                    x.Clave == eventoClave &&
                    x.Activo == true)
                .Select(x => new
                {
                    x.Id,
                    x.Clave
                })
                .FirstOrDefaultAsync(ct);

            if (evento == null)
            {
                throw new InvalidOperationException(
                    $"El evento '{eventoClave}' no está configurado o está inactivo.");
            }

            // ======================================
            // VALIDAR TICKET
            // ======================================

            if (dto.TicketId.HasValue)
            {
                var ticketExiste = await context.Tickets
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == dto.TicketId.Value &&
                        x.EmpresaId == dto.EmpresaId,
                        ct);

                if (!ticketExiste)
                {
                    throw new InvalidOperationException(
                        "El ticket no existe en la empresa indicada.");
                }
            }

            // ======================================
            // VERIFICAR IDEMPOTENCIA
            // ======================================

            var existente = await context
                .NotificacionEventoPendientes
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == dto.EmpresaId &&
                    x.ClaveIdempotencia == claveIdempotencia)
                .Select(x => new
                {
                    x.Id,
                    x.Estado,
                    x.NotificacionEventoId,
                    x.TicketId
                })
                .FirstOrDefaultAsync(ct);

            if (existente != null)
            {
                // La misma clave no debe reutilizarse
                // para otro evento o ticket.

                if (
                    existente.NotificacionEventoId != evento.Id ||
                    existente.TicketId != dto.TicketId
                )
                {
                    throw new InvalidOperationException(
                        "La clave de idempotencia ya pertenece a otra operación.");
                }

                return new NotificacionEventoResultadoDto
                {
                    Id = existente.Id,
                    Creado = false,
                    Estado = existente.Estado
                };
            }

            // ======================================
            // CREAR EVENTO PENDIENTE
            // ======================================

            var ahora = DateTime.UtcNow;

            var entidad = new NotificacionEventoPendiente
            {
                Id = Guid.NewGuid(),

                EmpresaId = dto.EmpresaId,

                NotificacionEventoId = evento.Id,

                TicketId = dto.TicketId,

                ClaveIdempotencia = claveIdempotencia,

                PayloadJson = payloadJson,

                Estado = "PENDIENTE",

                Intentos = 0,

                MaxIntentos = 5,

                FechaProximoIntento = ahora,

                FechaBloqueo = null,

                BloqueoToken = null,

                FechaProcesamiento = null,

                UltimoError = null,

                FechaCreacion = ahora,

                UsuarioCreacion = dto.UsuarioCreacion,

                FechaActualizacion = null
            };

            context.NotificacionEventoPendientes.Add(entidad);

            // ======================================
            // GUARDAR
            // ======================================

            try
            {
                await context.SaveChangesAsync(ct);

                logger.LogInformation(
                    "Evento {EventoClave} registrado. " +
                    "Empresa: {EmpresaId}, Ticket: {TicketId}, EventoId: {EventoId}",
                    eventoClave,
                    dto.EmpresaId,
                    dto.TicketId,
                    entidad.Id);

                return new NotificacionEventoResultadoDto
                {
                    Id = entidad.Id,
                    Creado = true,
                    Estado = entidad.Estado
                };
            }
            catch (DbUpdateException ex)
                when (EsErrorDuplicado(ex))
            {
                // ==================================
                // COLISION POR CONCURRENCIA
                // ==================================

                // Dos solicitudes pueden intentar
                // insertar el mismo evento al mismo
                // tiempo. El índice único de SQL
                // garantiza que solo una lo consiga.

                context.Entry(entidad).State =
                    EntityState.Detached;

                var duplicado = await context
                    .NotificacionEventoPendientes
                    .AsNoTracking()
                    .Where(x =>
                        x.EmpresaId == dto.EmpresaId &&
                        x.ClaveIdempotencia == claveIdempotencia)
                    .Select(x => new
                    {
                        x.Id,
                        x.Estado,
                        x.NotificacionEventoId,
                        x.TicketId
                    })
                    .FirstOrDefaultAsync(ct);

                if (duplicado == null)
                {
                    throw;
                }

                if (
                    duplicado.NotificacionEventoId != evento.Id ||
                    duplicado.TicketId != dto.TicketId
                )
                {
                    throw new InvalidOperationException(
                        "La clave de idempotencia ya pertenece a otra operación.");
                }

                logger.LogInformation(
                    "Evento duplicado detectado. " +
                    "Empresa: {EmpresaId}, Clave: {Clave}",
                    dto.EmpresaId,
                    claveIdempotencia);

                return new NotificacionEventoResultadoDto
                {
                    Id = duplicado.Id,
                    Creado = false,
                    Estado = duplicado.Estado
                };
            }
        }

        // ==========================================
        // IDENTIFICAR ERROR DE CLAVE DUPLICADA
        // ==========================================

        private static bool EsErrorDuplicado(
            DbUpdateException ex)
        {
            return ex.InnerException is SqlException sqlEx &&
                   (sqlEx.Number == 2601 ||
                    sqlEx.Number == 2627);
        }
    }
}
