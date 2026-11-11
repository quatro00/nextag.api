using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto;
using Nextag.Api.Models.Dto.Tickets;
using Nextag.Api.Services.Tickets.Interface;
using System.Data;

namespace Nextag.Api.Services.Tickets.Implementation
{
    public class TicketAsignacionService : ITicketAsignacionService
    {
        private readonly NextagContext context;

        public TicketAsignacionService(NextagContext context)
        {
            this.context = context;
        }

        // ==========================================
        // CONSULTAR ASIGNACION VIGENTE
        // ==========================================

        public async Task<TicketAsignacionDto?> GetVigenteAsync(
            Guid ticketId,
            Guid empresaId,
            CancellationToken ct = default)
        {
            return await ConsultaAsignaciones(empresaId)
                .Where(x =>
                    x.TicketId == ticketId &&
                    x.FechaFin == null)
                .Select(x => new TicketAsignacionDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,

                    TicketId = x.TicketId,
                    TicketFolio = x.Ticket.Folio,
                    TicketTitulo = x.Ticket.Titulo,

                    EquipoSoporteId = x.EquipoSoporteId,
                    EquipoSoporteNombre = x.E.Nombre,
                    EquipoSoporteTipo = x.E.Tipo,

                    UsuarioTecnicoId = x.UsuarioTecnicoId,

                    UsuarioTecnicoNombre = x.UsuarioTecnico != null
                        ? x.UsuarioTecnico.Nombre + " " +
                          x.UsuarioTecnico.Apellidos
                        : null,

                    UsuarioTecnicoCorreo = x.UsuarioTecnico != null
                        ? x.UsuarioTecnico.Email
                        : null,

                    FechaAsignacion = x.FechaAsignacion,
                    FechaFin = x.FechaFin,

                    Motivo = x.Motivo,
                    UsuarioCreacion = x.UsuarioCreacion,

                    Vigente = true
                })
                .FirstOrDefaultAsync(ct);
        }

        // ==========================================
        // HISTORIAL DE ASIGNACIONES
        // ==========================================

        public async Task<List<TicketAsignacionDto>> GetHistorialAsync(
            Guid ticketId,
            Guid empresaId,
            CancellationToken ct = default)
        {
            return await ConsultaAsignaciones(empresaId)
                .Where(x => x.TicketId == ticketId)
                .OrderByDescending(x => x.FechaAsignacion)
                .Select(x => new TicketAsignacionDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,

                    TicketId = x.TicketId,
                    TicketFolio = x.Ticket.Folio,
                    TicketTitulo = x.Ticket.Titulo,

                    EquipoSoporteId = x.EquipoSoporteId,
                    EquipoSoporteNombre = x.E.Nombre,
                    EquipoSoporteTipo = x.E.Tipo,

                    UsuarioTecnicoId = x.UsuarioTecnicoId,

                    UsuarioTecnicoNombre = x.UsuarioTecnico != null
                        ? x.UsuarioTecnico.Nombre + " " +
                          x.UsuarioTecnico.Apellidos
                        : null,

                    UsuarioTecnicoCorreo = x.UsuarioTecnico != null
                        ? x.UsuarioTecnico.Email
                        : null,

                    FechaAsignacion = x.FechaAsignacion,
                    FechaFin = x.FechaFin,

                    Motivo = x.Motivo,
                    UsuarioCreacion = x.UsuarioCreacion,

                    Vigente = x.FechaFin == null
                })
                .ToListAsync(ct);
        }

        // ==========================================
        // CONSULTA BASE
        // ==========================================

        private IQueryable<TicketAsignacion> ConsultaAsignaciones(
            Guid empresaId)
        {
            return context.TicketAsignacions
                .AsNoTracking()
                .Where(x => x.EmpresaId == empresaId);
        }

        // ==========================================
        // PRIMERA ASIGNACION
        // ==========================================

        public async Task<ServiceResult<TicketAsignacionDto>> AsignarAsync(
            Guid ticketId,
            Guid empresaId,
            Guid usuarioId,
            TicketAsignarDto dto,
            CancellationToken ct = default)
        {
            return await GuardarAsignacionAsync(
                ticketId,
                empresaId,
                usuarioId,
                dto.EquipoSoporteId,
                dto.UsuarioTecnicoId,
                dto.Motivo,
                false,
                ct);
        }

        // ==========================================
        // REASIGNAR TICKET
        // ==========================================

        public async Task<ServiceResult<TicketAsignacionDto>> ReasignarAsync(
            Guid ticketId,
            Guid empresaId,
            Guid usuarioId,
            TicketReasignarDto dto,
            CancellationToken ct = default)
        {
            var motivo = dto.Motivo?.Trim();

            if (string.IsNullOrWhiteSpace(motivo) ||
                motivo.Length < 5 ||
                motivo.Length > 500)
            {
                return ServiceResult<TicketAsignacionDto>.Fail(
                    "El motivo debe contener entre 5 y 500 caracteres."
                );
            }

            return await GuardarAsignacionAsync(
                ticketId,
                empresaId,
                usuarioId,
                dto.EquipoSoporteId,
                dto.UsuarioTecnicoId,
                motivo,
                true,
                ct);
        }

        // ==========================================
        // GUARDAR ASIGNACION / REASIGNACION
        // ==========================================

        private async Task<ServiceResult<TicketAsignacionDto>>
            GuardarAsignacionAsync(
                Guid ticketId,
                Guid empresaId,
                Guid usuarioId,
                Guid equipoSoporteId,
                Guid? usuarioTecnicoId,
                string? motivo,
                bool esReasignacion,
                CancellationToken ct)
        {
            if (ticketId == Guid.Empty ||
                empresaId == Guid.Empty ||
                usuarioId == Guid.Empty ||
                equipoSoporteId == Guid.Empty)
            {
                return ServiceResult<TicketAsignacionDto>.Fail(
                    "Los identificadores proporcionados no son válidos."
                );
            }

            motivo = motivo?.Trim();

            if (motivo?.Length > 500)
            {
                return ServiceResult<TicketAsignacionDto>.Fail(
                    "El motivo no puede superar 500 caracteres."
                );
            }

            // ======================================
            // VALIDAR EQUIPO
            // ======================================

            var equipo = await context.EquipoSoportes
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == equipoSoporteId &&
                    x.EmpresaId == empresaId &&
                    x.Activo == true,
                    ct);

            if (equipo == null)
            {
                return ServiceResult<TicketAsignacionDto>.Fail(
                    "El equipo de soporte no existe o está inactivo."
                );
            }

            // ======================================
            // VALIDAR TECNICO
            // ======================================

            if (usuarioTecnicoId.HasValue)
            {
                var tecnicoValido =
                    await context.EquipoSoporteIntegrantes
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.EmpresaId == empresaId &&
                            x.EquipoSoporteId == equipoSoporteId &&
                            x.UsuarioId == usuarioTecnicoId.Value &&
                            x.Activo == true &&
                            x.Usuario.Activo == true &&
                            x.Usuario.UsuarioEmpresas.Any(ue =>
                                ue.EmpresaId == empresaId &&
                                ue.Activo == true),
                            ct);

                if (!tecnicoValido)
                {
                    return ServiceResult<TicketAsignacionDto>.Fail(
                        "El técnico no pertenece al equipo de soporte " +
                        "o no se encuentra activo en la empresa."
                    );
                }
            }

            // ======================================
            // TRANSACCION
            // ======================================

            await using var transaction =
                await context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    ct);

            // Mismo criterio de bloqueo utilizado
            // en TicketEstadoService.

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
                return ServiceResult<TicketAsignacionDto>.Fail(
                    "Ticket no encontrado."
                );
            }

            var estadoActual = await context.EstadoTickets
                .AsNoTracking()
                .Where(x => x.Id == ticket.EstadoTicketId)
                .Select(x => x.Clave)
                .FirstOrDefaultAsync(ct);

            // ======================================
            // VALIDAR ESTADO
            // ======================================

            if (estadoActual is not
                ("ABIERTO" or "ASIGNADO" or
                 "EN_PROCESO" or "REABIERTO"))
            {
                return ServiceResult<TicketAsignacionDto>.Fail(
                    "El ticket no permite asignaciones en su estado actual."
                );
            }

            // ======================================
            // ASIGNACION ANTERIOR
            // ======================================

            var asignacionAnterior =
                await context.TicketAsignacions
                    .FirstOrDefaultAsync(x =>
                        x.EmpresaId == empresaId &&
                        x.TicketId == ticketId &&
                        x.FechaFin == null,
                        ct);

            if (!esReasignacion && asignacionAnterior != null)
            {
                return ServiceResult<TicketAsignacionDto>.Fail(
                    "El ticket ya tiene una asignación vigente."
                );
            }

            if (esReasignacion && asignacionAnterior == null)
            {
                return ServiceResult<TicketAsignacionDto>.Fail(
                    "El ticket no tiene una asignación vigente para reasignar."
                );
            }

            // Impedir registrar exactamente la misma
            // combinación de equipo y técnico.

            if (asignacionAnterior != null &&
                asignacionAnterior.EquipoSoporteId == equipoSoporteId &&
                asignacionAnterior.UsuarioTecnicoId == usuarioTecnicoId)
            {
                return ServiceResult<TicketAsignacionDto>.Fail(
                    "El ticket ya está asignado a ese equipo y técnico."
                );
            }

            var ahora = DateTime.UtcNow;

            // ======================================
            // FINALIZAR ASIGNACION ANTERIOR
            // ======================================

            if (asignacionAnterior != null)
            {
                asignacionAnterior.FechaFin = ahora;
            }

            // ======================================
            // CREAR NUEVA ASIGNACION
            // ======================================

            var asignacion = new TicketAsignacion
            {
                Id = Guid.NewGuid(),

                EmpresaId = empresaId,
                TicketId = ticketId,

                EquipoSoporteId = equipoSoporteId,
                UsuarioTecnicoId = usuarioTecnicoId,

                FechaAsignacion = ahora,
                FechaFin = null,

                Motivo = motivo,

                UsuarioCreacion = usuarioId
            };

            context.TicketAsignacions.Add(asignacion);

            // ======================================
            // ACTUALIZAR ESTADO
            // ======================================

            var estadoAsignado =
                await context.EstadoTickets
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Clave == "ASIGNADO" &&
                        x.Activo == true,
                        ct);

            if (estadoAsignado == null)
            {
                return ServiceResult<TicketAsignacionDto>.Fail(
                    "No está configurado el estado ASIGNADO."
                );
            }

            var estadoAnteriorId = ticket.EstadoTicketId;

            ticket.EstadoTicketId = estadoAsignado.Id;
            ticket.FechaActualizacion = ahora;
            ticket.UsuarioModificacion = usuarioId;

            // ======================================
            // HISTORIAL DE ESTADOS
            // ======================================

            if (estadoAnteriorId != estadoAsignado.Id)
            {
                context.TicketEstadoHistorials.Add(
                    new TicketEstadoHistorial
                    {
                        Id = Guid.NewGuid(),

                        EmpresaId = empresaId,
                        TicketId = ticketId,

                        EstadoAnteriorId = estadoAnteriorId,
                        EstadoNuevoId = estadoAsignado.Id,

                        Motivo = esReasignacion
                            ? "Ticket reasignado."
                            : "Ticket asignado.",

                        FechaCambio = ahora,
                        UsuarioId = usuarioId
                    });
            }

            await context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);

            var resultado = await GetVigenteAsync(
                ticketId,
                empresaId,
                ct);

            return ServiceResult<TicketAsignacionDto>.Ok(
                resultado!);
        }

        // ==========================================
        // MIS TICKETS
        // ==========================================

        public async Task<List<TicketListDto>> GetMisTicketsAsync(
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default)
        {
            return await context.TicketAsignacions
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.UsuarioTecnicoId == usuarioId &&
                    x.FechaFin == null)
                .OrderByDescending(x => x.Ticket.FechaApertura)
                .Select(x => new TicketListDto
                {
                    Id = x.Ticket.Id,
                    EmpresaId = x.Ticket.EmpresaId,

                    Numero = x.Ticket.Numero,
                    Folio = x.Ticket.Folio,

                    Titulo = x.Ticket.Titulo,
                    Origen = x.Ticket.Origen,

                    ActivoId = x.Ticket.ActivoId,

                    ActivoCodigo = x.Ticket.Activo != null
                        ? x.Ticket.Activo.Codigo
                        : null,

                    ActivoNombre = x.Ticket.Activo != null
                        ? x.Ticket.Activo.Nombre
                        : null,

                    CategoriaTicketId = x.Ticket.CategoriaTicketId,

                    CategoriaNombre =
                        x.Ticket.CategoriaTicket != null
                            ? x.Ticket.CategoriaTicket.Nombre
                            : null,

                    PrioridadTicketId =
                        x.Ticket.PrioridadTicketId,

                    PrioridadNombre =
                        x.Ticket.PrioridadTicket.Nombre,

                    EstadoTicketId =
                        x.Ticket.EstadoTicketId,

                    EstadoNombre =
                        x.Ticket.EstadoTicket.Nombre,

                    SolicitanteUsuarioId =
                        x.Ticket.SolicitanteUsuarioId,

                    SolicitanteNombre =
                        x.Ticket.SolicitanteNombre,

                    FechaApertura =
                        x.Ticket.FechaApertura
                })
                .ToListAsync(ct);
        }

        // ==========================================
        // TICKETS DE MIS EQUIPOS
        // ==========================================

        public async Task<List<TicketListDto>> GetTicketsMisEquiposAsync(
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default)
        {
            var equiposUsuario =
                context.EquipoSoporteIntegrantes
                    .AsNoTracking()
                    .Where(x =>
                        x.EmpresaId == empresaId &&
                        x.UsuarioId == usuarioId &&
                        x.Activo == true &&
                        x.E.Activo == true &&
                        x.Usuario.Activo == true &&
                        x.Usuario.UsuarioEmpresas.Any(ue =>
                            ue.EmpresaId == empresaId &&
                            ue.Activo == true))
                    .Select(x => x.EquipoSoporteId);

            return await context.TicketAsignacions
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.FechaFin == null &&
                    equiposUsuario.Contains(x.EquipoSoporteId))
                .OrderByDescending(x => x.Ticket.FechaApertura)
                .Select(x => new TicketListDto
                {
                    Id = x.Ticket.Id,
                    EmpresaId = x.Ticket.EmpresaId,

                    Numero = x.Ticket.Numero,
                    Folio = x.Ticket.Folio,

                    Titulo = x.Ticket.Titulo,
                    Origen = x.Ticket.Origen,

                    ActivoId = x.Ticket.ActivoId,

                    ActivoCodigo = x.Ticket.Activo != null
                        ? x.Ticket.Activo.Codigo
                        : null,

                    ActivoNombre = x.Ticket.Activo != null
                        ? x.Ticket.Activo.Nombre
                        : null,

                    CategoriaTicketId = x.Ticket.CategoriaTicketId,

                    CategoriaNombre =
                        x.Ticket.CategoriaTicket != null
                            ? x.Ticket.CategoriaTicket.Nombre
                            : null,

                    PrioridadTicketId =
                        x.Ticket.PrioridadTicketId,

                    PrioridadNombre =
                        x.Ticket.PrioridadTicket.Nombre,

                    EstadoTicketId =
                        x.Ticket.EstadoTicketId,

                    EstadoNombre =
                        x.Ticket.EstadoTicket.Nombre,

                    SolicitanteUsuarioId =
                        x.Ticket.SolicitanteUsuarioId,

                    SolicitanteNombre =
                        x.Ticket.SolicitanteNombre,

                    FechaApertura =
                        x.Ticket.FechaApertura
                })
                .ToListAsync(ct);
        }
    }
}
