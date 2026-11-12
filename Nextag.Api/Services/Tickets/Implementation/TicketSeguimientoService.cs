using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Dto.Tickets;
using Nextag.Api.Models.Dto;
using Nextag.Api.Services.Tickets.Interface;
using Nextag.Api.Models.Domain;

namespace Nextag.Api.Services.Tickets.Implementation
{
    public class TicketSeguimientoService : ITicketSeguimientoService
    {
        private readonly NextagContext context;

        private const string TIPO_COMENTARIO = "COMENTARIO";
        private const string TIPO_NOTA_INTERNA = "NOTA_INTERNA";
        private const string TIPO_SOLUCION = "SOLUCION";

        public TicketSeguimientoService(NextagContext context)
        {
            this.context = context;
        }


        // ==========================================
        // LISTADO DE SEGUIMIENTO
        // ==========================================

        public async Task<ServiceResult<List<TicketSeguimientoDto>>>
            GetAllAsync(
                Guid ticketId,
                Guid empresaId,
                Guid usuarioId,
                bool esAdministrador,
                CancellationToken ct = default)
        {
            var ticketExiste = await context.Tickets
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == ticketId &&
                    x.EmpresaId == empresaId,
                    ct);

            if (!ticketExiste)
            {
                return ServiceResult<List<TicketSeguimientoDto>>.Fail(
                    "Ticket no encontrado."
                );
            }

            var acceso = await ObtenerPermisosAsync(
                ticketId,
                empresaId,
                usuarioId,
                esAdministrador,
                ct);

            if (!acceso.PuedeConsultar)
            {
                return ServiceResult<List<TicketSeguimientoDto>>.Fail(
                    "No tienes permiso para consultar este seguimiento."
                );
            }

            var query = context.TicketSeguimientos
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.TicketId == ticketId);

            // Las notas internas solamente son visibles
            // para administradores y soporte autorizado.

            if (!acceso.PuedeVerNotasInternas)
            {
                query = query.Where(x => x.VisibleSolicitante);
            }

            var registros = await query
                .OrderBy(x => x.FechaCreacion)
                .ThenBy(x => x.Id)
                .Select(x => new TicketSeguimientoDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,
                    TicketId = x.TicketId,

                    Tipo = x.Tipo,
                    Comentario = x.Comentario,

                    VisibleSolicitante = x.VisibleSolicitante,

                    AutorUsuarioId = x.AutorUsuarioId,

                    AutorNombre = x.AutorNombre ??
                        (x.AutorUsuario != null
                            ? x.AutorUsuario.Nombre + " " +
                              x.AutorUsuario.Apellidos
                            : null),

                    FechaCreacion = x.FechaCreacion
                })
                .ToListAsync(ct);

            return ServiceResult<List<TicketSeguimientoDto>>.Ok(
                registros);
        }

        // ==========================================
        // DETALLE DE SEGUIMIENTO
        // ==========================================

        public async Task<ServiceResult<TicketSeguimientoDto>>
            GetByIdAsync(
                Guid ticketId,
                Guid seguimientoId,
                Guid empresaId,
                Guid usuarioId,
                bool esAdministrador,
                CancellationToken ct = default)
        {
            var ticketExiste = await context.Tickets
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == ticketId &&
                    x.EmpresaId == empresaId,
                    ct);

            if (!ticketExiste)
            {
                return ServiceResult<TicketSeguimientoDto>.Fail(
                    "Ticket no encontrado."
                );
            }

            var acceso = await ObtenerPermisosAsync(
                ticketId,
                empresaId,
                usuarioId,
                esAdministrador,
                ct);

            if (!acceso.PuedeConsultar)
            {
                return ServiceResult<TicketSeguimientoDto>.Fail(
                    "No tienes permiso para consultar este seguimiento."
                );
            }

            var query = context.TicketSeguimientos
                .AsNoTracking()
                .Where(x =>
                    x.Id == seguimientoId &&
                    x.TicketId == ticketId &&
                    x.EmpresaId == empresaId);

            if (!acceso.PuedeVerNotasInternas)
            {
                query = query.Where(x => x.VisibleSolicitante);
            }

            var registro = await query
                .Select(x => new TicketSeguimientoDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,
                    TicketId = x.TicketId,

                    Tipo = x.Tipo,
                    Comentario = x.Comentario,

                    VisibleSolicitante = x.VisibleSolicitante,

                    AutorUsuarioId = x.AutorUsuarioId,

                    AutorNombre = x.AutorNombre ??
                        (x.AutorUsuario != null
                            ? x.AutorUsuario.Nombre + " " +
                              x.AutorUsuario.Apellidos
                            : null),

                    FechaCreacion = x.FechaCreacion
                })
                .FirstOrDefaultAsync(ct);

            if (registro == null)
            {
                return ServiceResult<TicketSeguimientoDto>.Fail(
                    "Seguimiento no encontrado."
                );
            }

            return ServiceResult<TicketSeguimientoDto>.Ok(
                registro);
        }

        // ==========================================
        // AGREGAR COMENTARIO
        // ==========================================

        public async Task<ServiceResult<TicketSeguimientoDto>>
            AgregarComentarioAsync(
                Guid ticketId,
                Guid empresaId,
                Guid usuarioId,
                bool esAdministrador,
                TicketSeguimientoCrearDto dto,
                CancellationToken ct = default)
        {
            return await CrearSeguimientoAsync(
                ticketId,
                empresaId,
                usuarioId,
                esAdministrador,
                dto,
                TIPO_COMENTARIO,
                true,
                ct);
        }

        // ==========================================
        // AGREGAR NOTA INTERNA
        // ==========================================

        public async Task<ServiceResult<TicketSeguimientoDto>>
            AgregarNotaInternaAsync(
                Guid ticketId,
                Guid empresaId,
                Guid usuarioId,
                bool esAdministrador,
                TicketSeguimientoCrearDto dto,
                CancellationToken ct = default)
        {
            return await CrearSeguimientoAsync(
                ticketId,
                empresaId,
                usuarioId,
                esAdministrador,
                dto,
                TIPO_NOTA_INTERNA,
                false,
                ct);
        }

        // ==========================================
        // CREAR SEGUIMIENTO
        // ==========================================

        private async Task<ServiceResult<TicketSeguimientoDto>>
            CrearSeguimientoAsync(
                Guid ticketId,
                Guid empresaId,
                Guid usuarioId,
                bool esAdministrador,
                TicketSeguimientoCrearDto dto,
                string tipo,
                bool visibleSolicitante,
                CancellationToken ct)
        {
            if (ticketId == Guid.Empty ||
                empresaId == Guid.Empty ||
                usuarioId == Guid.Empty)
            {
                return ServiceResult<TicketSeguimientoDto>.Fail(
                    "Los identificadores proporcionados no son válidos."
                );
            }

            var comentario = dto.Comentario?.Trim();

            if (string.IsNullOrWhiteSpace(comentario) ||
                comentario.Length < 3 ||
                comentario.Length > 10000)
            {
                return ServiceResult<TicketSeguimientoDto>.Fail(
                    "El comentario debe contener entre 3 y 10000 caracteres."
                );
            }

            // ======================================
            // VALIDAR TICKET
            // ======================================

            var ticket = await context.Tickets
                .AsNoTracking()
                .Where(x =>
                    x.Id == ticketId &&
                    x.EmpresaId == empresaId)
                .Select(x => new
                {
                    x.Id,
                    EstadoClave = x.EstadoTicket.Clave
                })
                .FirstOrDefaultAsync(ct);

            if (ticket == null)
            {
                return ServiceResult<TicketSeguimientoDto>.Fail(
                    "Ticket no encontrado."
                );
            }

            if (ticket.EstadoClave is "CERRADO" or "CANCELADO")
            {
                return ServiceResult<TicketSeguimientoDto>.Fail(
                    "No se pueden agregar comentarios a tickets cerrados o cancelados."
                );
            }

            // ======================================
            // VALIDAR PERMISOS
            // ======================================

            var acceso = await ObtenerPermisosAsync(
                ticketId,
                empresaId,
                usuarioId,
                esAdministrador,
                ct);

            if (!acceso.PuedeEscribir)
            {
                return ServiceResult<TicketSeguimientoDto>.Fail(
                    "No tienes permiso para registrar seguimiento en este ticket."
                );
            }

            // ======================================
            // OBTENER AUTOR
            // ======================================

            var autor = await context.Usuarios
                .AsNoTracking()
                .Where(x =>
                    x.Id == usuarioId &&
                    x.Activo == true &&
                    x.UsuarioEmpresas.Any(ue =>
                        ue.EmpresaId == empresaId &&
                        ue.Activo == true))
                .Select(x => new
                {
                    x.Id,
                    x.Nombre,
                    x.Apellidos
                })
                .FirstOrDefaultAsync(ct);

            if (autor == null)
            {
                return ServiceResult<TicketSeguimientoDto>.Fail(
                    "El usuario no está activo en la empresa."
                );
            }

            var nombreAutor =
                $"{autor.Nombre} {autor.Apellidos}".Trim();

            var ahora = DateTime.UtcNow;

            // ======================================
            // CREAR REGISTRO
            // ======================================

            var seguimiento = new TicketSeguimiento
            {
                Id = Guid.NewGuid(),

                EmpresaId = empresaId,
                TicketId = ticketId,

                Tipo = tipo,
                Comentario = comentario,

                VisibleSolicitante = visibleSolicitante,

                AutorUsuarioId = usuarioId,
                AutorNombre = nombreAutor,

                FechaCreacion = ahora
            };

            context.TicketSeguimientos.Add(seguimiento);

            await context.SaveChangesAsync(ct);

            // ======================================
            // RESPUESTA
            // ======================================

            var resultado = new TicketSeguimientoDto
            {
                Id = seguimiento.Id,

                EmpresaId = seguimiento.EmpresaId,
                TicketId = seguimiento.TicketId,

                Tipo = seguimiento.Tipo,
                Comentario = seguimiento.Comentario,

                VisibleSolicitante =
                    seguimiento.VisibleSolicitante,

                AutorUsuarioId = seguimiento.AutorUsuarioId,
                AutorNombre = seguimiento.AutorNombre,

                FechaCreacion = seguimiento.FechaCreacion
            };

            return ServiceResult<TicketSeguimientoDto>.Ok(
                resultado);
        }

        // ==========================================
        // PERMISOS DE SEGUIMIENTO
        // ==========================================

        private sealed class PermisosSeguimiento
        {
            public bool PuedeConsultar { get; set; }

            public bool PuedeVerNotasInternas { get; set; }

            public bool PuedeEscribir { get; set; }
        }

        private async Task<PermisosSeguimiento>
            ObtenerPermisosAsync(
                Guid ticketId,
                Guid empresaId,
                Guid usuarioId,
                bool esAdministrador,
                CancellationToken ct)
        {
            var permisos = new PermisosSeguimiento
            {
                // Se conserva el acceso de lectura
                // por empresa de los entregables anteriores.
                PuedeConsultar = true,

                PuedeVerNotasInternas = esAdministrador,

                PuedeEscribir = esAdministrador
            };

            if (esAdministrador)
            {
                return permisos;
            }

            // ======================================
            // ASIGNACION VIGENTE
            // ======================================

            var asignacion = await context.TicketAsignacions
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.TicketId == ticketId &&
                    x.FechaFin == null)
                .Select(x => new
                {
                    x.EquipoSoporteId,
                    x.UsuarioTecnicoId
                })
                .FirstOrDefaultAsync(ct);

            if (asignacion == null)
            {
                return permisos;
            }

            // ======================================
            // INTEGRANTE ACTIVO
            // ======================================

            var perteneceEquipo =
                await context.EquipoSoporteIntegrantes
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.EmpresaId == empresaId &&
                        x.EquipoSoporteId ==
                            asignacion.EquipoSoporteId &&
                        x.UsuarioId == usuarioId &&
                        x.Activo == true &&
                        x.E.Activo == true &&
                        x.Usuario.Activo == true &&
                        x.Usuario.UsuarioEmpresas.Any(ue =>
                            ue.EmpresaId == empresaId &&
                            ue.Activo == true),
                        ct);

            if (!perteneceEquipo)
            {
                return permisos;
            }

            // Los integrantes del equipo pueden
            // consultar sus notas internas.

            permisos.PuedeVerNotasInternas = true;

            // Si hay un técnico responsable,
            // solamente ese técnico puede escribir.
            //
            // Si no hay técnico, cualquier integrante
            // activo del equipo puede registrar seguimiento.

            permisos.PuedeEscribir =
                asignacion.UsuarioTecnicoId == null ||
                asignacion.UsuarioTecnicoId == usuarioId;

            return permisos;
        }
    }
}
