using Nextag.Api.Data;
using Nextag.Api.Models.Dto.Tickets;
using Nextag.Api.Models.Dto;
using Nextag.Api.Services.Tickets.Interface;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Models.Domain;
using System.Data;

namespace Nextag.Api.Services.Tickets.Implementation
{
    public class TicketEstadoService : ITicketEstadoService
    {
        private readonly NextagContext context;
        private readonly ITicketService ticketService;

        public TicketEstadoService(
            NextagContext context,
            ITicketService ticketService)
        {
            this.context = context;
            this.ticketService = ticketService;
        }

        // ==========================================
        // HISTORIAL DE ESTADOS
        // ==========================================

        public async Task<List<TicketEstadoHistorialDto>>
            GetHistorialAsync(
                Guid ticketId,
                Guid empresaId,
                CancellationToken ct = default)
        {
            return await context.TicketEstadoHistorials
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.TicketId == ticketId)
                .OrderBy(x => x.FechaCambio)
                .ThenBy(x => x.Id)
                .Select(x => new TicketEstadoHistorialDto
                {
                    Id = x.Id,
                    TicketId = x.TicketId,

                    EstadoAnteriorId = x.EstadoAnteriorId,

                    EstadoAnteriorNombre =
                        x.EstadoAnterior != null
                            ? x.EstadoAnterior.Nombre
                            : null,

                    EstadoNuevoId = x.EstadoNuevoId,
                    EstadoNuevoNombre = x.EstadoNuevo.Nombre,

                    Motivo = x.Motivo,

                    FechaCambio = x.FechaCambio,
                    UsuarioId = x.UsuarioId
                })
                .ToListAsync(ct);
        }

        // ==========================================
        // INICIAR ATENCION
        // ==========================================

        public async Task<ServiceResult<TicketDetalleDto>> IniciarAsync(
            Guid ticketId,
            Guid empresaId,
            Guid usuarioId,
            bool esAdministrador,
            CancellationToken ct = default)
        {
            return await CambiarEstadoAsync(
                ticketId,
                empresaId,
                usuarioId,
                "EN_PROCESO",
                null,
                esAdministrador,
                ct);
        }

        // ==========================================
        // RESOLVER
        // ==========================================

        public async Task<ServiceResult<TicketDetalleDto>> ResolverAsync(
            Guid ticketId,
            Guid empresaId,
            Guid usuarioId,
            bool esAdministrador,
            TicketResolverDto dto,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Solucion) ||
                dto.Solucion.Trim().Length < 5)
            {
                return ServiceResult<TicketDetalleDto>.Fail(
                    "Debes proporcionar una descripción de la solución."
                );
            }

            return await CambiarEstadoAsync(
                ticketId,
                empresaId,
                usuarioId,
                "RESUELTO",
                dto.Solucion.Trim(),
                esAdministrador,
                ct);
        }

        // ==========================================
        // CERRAR
        // ==========================================

        public async Task<ServiceResult<TicketDetalleDto>> CerrarAsync(
            Guid ticketId,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default)
        {
            return await CambiarEstadoAsync(
                ticketId,
                empresaId,
                usuarioId,
                "CERRADO",
                null,
                true,
                ct);
        }

        // ==========================================
        // REABRIR
        // ==========================================

        public async Task<ServiceResult<TicketDetalleDto>> ReabrirAsync(
            Guid ticketId,
            Guid empresaId,
            Guid usuarioId,
            TicketReabrirDto dto,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Motivo) ||
                dto.Motivo.Trim().Length < 5)
            {
                return ServiceResult<TicketDetalleDto>.Fail(
                    "Debes proporcionar el motivo de reapertura."
                );
            }

            return await CambiarEstadoAsync(
                ticketId,
                empresaId,
                usuarioId,
                "REABIERTO",
                dto.Motivo.Trim(),
                true,
                ct);
        }

        // ==========================================
        // CANCELAR
        // ==========================================

        public async Task<ServiceResult<TicketDetalleDto>> CancelarAsync(
            Guid ticketId,
            Guid empresaId,
            Guid usuarioId,
            TicketCancelarDto dto,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Motivo) ||
                dto.Motivo.Trim().Length < 5)
            {
                return ServiceResult<TicketDetalleDto>.Fail(
                    "Debes proporcionar el motivo de cancelación."
                );
            }

            return await CambiarEstadoAsync(
                ticketId,
                empresaId,
                usuarioId,
                "CANCELADO",
                dto.Motivo.Trim(),
                true,
                ct);
        }

        // ==========================================
        // MOTOR DE CAMBIOS DE ESTADO
        // ==========================================

        private async Task<ServiceResult<TicketDetalleDto>>
            CambiarEstadoAsync(
                Guid ticketId,
                Guid empresaId,
                Guid usuarioId,
                string estadoDestinoClave,
                string? motivo,
                bool esAdministrador,
                CancellationToken ct)
        {
            if (ticketId == Guid.Empty ||
                empresaId == Guid.Empty ||
                usuarioId == Guid.Empty)
            {
                return ServiceResult<TicketDetalleDto>.Fail(
                    "Los identificadores proporcionados no son válidos."
                );
            }

            // La transacción serializable y el bloqueo
            // UPDLOCK/HOLDLOCK protegen el ticket ante
            // cambios de estado simultáneos.

            await using var transaction =
                await context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    ct);

            var ticket = await context.Tickets
                .FromSqlInterpolated($"""
                SELECT *
                FROM dbo.Ticket WITH (UPDLOCK, HOLDLOCK)
                WHERE Id = {ticketId}
                  AND EmpresaId = {empresaId}
                """)
                .FirstOrDefaultAsync(ct);

            if (ticket == null)
            {
                return ServiceResult<TicketDetalleDto>.Fail(
                    "Ticket no encontrado."
                );
            }

            var estadoActual = await context.EstadoTickets
                .AsNoTracking()
                .Where(x => x.Id == ticket.EstadoTicketId)
                .Select(x => x.Clave)
                .FirstOrDefaultAsync(ct);

            if (estadoActual == null)
            {
                return ServiceResult<TicketDetalleDto>.Fail(
                    "El ticket tiene un estado no válido."
                );
            }

            var transicionValida = ValidarTransicion(
                estadoActual,
                estadoDestinoClave);

            if (!transicionValida)
            {
                return ServiceResult<TicketDetalleDto>.Fail(
                    $"No se permite cambiar el ticket de " +
                    $"{estadoActual} a {estadoDestinoClave}."
                );
            }

            var estadoDestino = await context.EstadoTickets
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Clave == estadoDestinoClave &&
                    x.Activo == true,
                    ct);

            if (estadoDestino == null)
            {
                return ServiceResult<TicketDetalleDto>.Fail(
                    $"No está configurado el estado {estadoDestinoClave}."
                );
            }

            // ======================================
            // VALIDAR ASIGNACION Y PERMISOS
            // ======================================

            if (estadoDestinoClave is "EN_PROCESO" or "RESUELTO")
            {
                var asignacionVigente = await context.TicketAsignacions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.EmpresaId == empresaId &&
                        x.TicketId == ticketId &&
                        x.FechaFin == null,
                        ct);

                if (asignacionVigente == null)
                {
                    return ServiceResult<TicketDetalleDto>.Fail(
                        "El ticket debe tener una asignación vigente."
                    );
                }

                if (!esAdministrador)
                {
                    if (asignacionVigente.UsuarioTecnicoId != usuarioId)
                    {
                        return ServiceResult<TicketDetalleDto>.Fail(
                            "No tienes permiso para atender este ticket."
                        );
                    }

                    var integranteActivo =
                        await context.EquipoSoporteIntegrantes
                            .AsNoTracking()
                            .AnyAsync(x =>
                                x.EmpresaId == empresaId &&
                                x.EquipoSoporteId ==
                                    asignacionVigente.EquipoSoporteId &&
                                x.UsuarioId == usuarioId &&
                                x.Activo == true,
                                ct);

                    if (!integranteActivo)
                    {
                        return ServiceResult<TicketDetalleDto>.Fail(
                            "No eres integrante activo del equipo asignado."
                        );
                    }
                }
            }

            // ======================================
            // FECHAS Y DATOS
            // ======================================

            var ahora = DateTime.UtcNow;
            var estadoAnteriorId = ticket.EstadoTicketId;

            switch (estadoDestinoClave)
            {
                case "EN_PROCESO":

                    ticket.FechaPrimeraAtencion ??= ahora;

                    break;

                case "RESUELTO":

                    ticket.FechaResolucion = ahora;
                    ticket.FechaCierre = null;

                    break;

                case "CERRADO":

                    ticket.FechaCierre = ahora;

                    break;

                case "REABIERTO":

                    ticket.FechaResolucion = null;
                    ticket.FechaCierre = null;
                    ticket.FechaCancelacion = null;
                    ticket.MotivoCancelacion = null;

                    break;

                case "CANCELADO":

                    ticket.FechaCancelacion = ahora;
                    ticket.MotivoCancelacion = motivo;

                    break;
            }

            ticket.EstadoTicketId = estadoDestino.Id;
            ticket.FechaActualizacion = ahora;
            ticket.UsuarioModificacion = usuarioId;

            // ======================================
            // HISTORIAL
            // ======================================

            var historial = new TicketEstadoHistorial
            {
                Id = Guid.NewGuid(),

                EmpresaId = empresaId,
                TicketId = ticketId,

                EstadoAnteriorId = estadoAnteriorId,
                EstadoNuevoId = estadoDestino.Id,

                Motivo = ObtenerMotivoHistorial(
                    estadoDestinoClave,
                    motivo),

                FechaCambio = ahora,
                UsuarioId = usuarioId
            };

            context.TicketEstadoHistorials.Add(historial);

            // ======================================
            // REGISTRAR SOLUCION
            // ======================================

            if (estadoDestinoClave == "RESUELTO")
            {
                var seguimiento = new TicketSeguimiento
                {
                    Id = Guid.NewGuid(),

                    EmpresaId = empresaId,
                    TicketId = ticketId,

                    Tipo = "SOLUCION",
                    Comentario = motivo!,

                    VisibleSolicitante = true,

                    AutorUsuarioId = usuarioId,
                    FechaCreacion = ahora
                };

                context.TicketSeguimientos.Add(seguimiento);
            }

            // ======================================
            // FINALIZAR ASIGNACIONES
            // ======================================

            // Al cerrar o cancelar, termina la
            // asignación vigente del ticket.
            //
            // Al resolver no la terminamos, para
            // permitir una reapertura inmediata.

            if (estadoDestinoClave is "CERRADO" or "CANCELADO")
            {
                var asignacionesVigentes =
                    await context.TicketAsignacions
                        .Where(x =>
                            x.EmpresaId == empresaId &&
                            x.TicketId == ticketId &&
                            x.FechaFin == null)
                        .ToListAsync(ct);

                foreach (var asignacion in asignacionesVigentes)
                {
                    asignacion.FechaFin = ahora;
                }
            }

            // ======================================
            // GUARDAR
            // ======================================

            await context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);

            var detalle = await ticketService.GetByIdAsync(
                ticketId,
                empresaId,
                ct);

            return ServiceResult<TicketDetalleDto>.Ok(detalle!);
        }

        // ==========================================
        // VALIDAR TRANSICION
        // ==========================================

        private static bool ValidarTransicion(
            string estadoActual,
            string estadoDestino)
        {
            return (estadoActual, estadoDestino) switch
            {
                ("ASIGNADO", "EN_PROCESO") => true,

                ("REABIERTO", "EN_PROCESO") => true,

                ("EN_PROCESO", "RESUELTO") => true,

                ("RESUELTO", "CERRADO") => true,

                ("RESUELTO", "REABIERTO") => true,

                ("CERRADO", "REABIERTO") => true,

                ("ABIERTO", "CANCELADO") => true,

                ("ASIGNADO", "CANCELADO") => true,

                ("EN_PROCESO", "CANCELADO") => true,

                ("REABIERTO", "CANCELADO") => true,

                _ => false
            };
        }

        // ==========================================
        // MOTIVO DEL HISTORIAL
        // ==========================================

        private static string ObtenerMotivoHistorial(
            string estadoDestino,
            string? motivo)
        {
            if (estadoDestino == "RESUELTO")
            {
                return "Ticket resuelto.";
            }

            if (!string.IsNullOrWhiteSpace(motivo))
            {
                return motivo.Trim();
            }

            return estadoDestino switch
            {
                "EN_PROCESO" => "Inicio de atención.",

                "CERRADO" => "Cierre del ticket.",

                _ => "Cambio de estado."
            };
        }
    }
}
