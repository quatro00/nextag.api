using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto.EquipoSoporteIntegrantes;
using Nextag.Api.Models.Dto;
using Nextag.Api.Services.EquipoSoporteIntegrantes.Interface;

namespace Nextag.Api.Services.EquipoSoporteIntegrantes.Implementation
{
    public class EquipoSoporteIntegranteService
    : IEquipoSoporteIntegranteService
    {
        private readonly NextagContext context;

        public EquipoSoporteIntegranteService(NextagContext context)
        {
            this.context = context;
        }

        // ==========================================
        // LISTADO
        // ==========================================

        public async Task<List<EquipoSoporteIntegranteDto>> GetAllAsync(
            Guid empresaId,
            Guid equipoId,
            bool? activo = null,
            CancellationToken ct = default)
        {
            var query = context.EquipoSoporteIntegrantes
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.EquipoSoporteId == equipoId);

            if (activo.HasValue)
            {
                query = query.Where(x =>
                    x.Activo == activo.Value);
            }

            return await query
                .OrderBy(x => x.Usuario.Nombre)
                .ThenBy(x => x.Usuario.Apellidos)
                .Select(x => new EquipoSoporteIntegranteDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,
                    EquipoSoporteId = x.EquipoSoporteId,
                    UsuarioId = x.UsuarioId,

                    UsuarioNombre =
                        x.Usuario.Nombre + " " +
                        x.Usuario.Apellidos,

                    UsuarioCorreo = x.Usuario.Email,

                    Activo = x.Activo == true,

                    FechaCreacion = x.FechaCreacion,
                    UsuarioCreacion = x.UsuarioCreacion,
                    FechaActualizacion = x.FechaActualizacion,
                    UsuarioModificacion = x.UsuarioModificacion
                })
                .ToListAsync(ct);
        }

        // ==========================================
        // DETALLE
        // ==========================================

        public async Task<EquipoSoporteIntegranteDto?> GetByIdAsync(
            Guid empresaId,
            Guid equipoId,
            Guid id,
            CancellationToken ct = default)
        {
            return await context.EquipoSoporteIntegrantes
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId &&
                    x.EquipoSoporteId == equipoId)
                .Select(x => new EquipoSoporteIntegranteDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,
                    EquipoSoporteId = x.EquipoSoporteId,
                    UsuarioId = x.UsuarioId,

                    UsuarioNombre =
                        x.Usuario.Nombre + " " +
                        x.Usuario.Apellidos,

                    UsuarioCorreo = x.Usuario.Email,

                    Activo = x.Activo == true,

                    FechaCreacion = x.FechaCreacion,
                    UsuarioCreacion = x.UsuarioCreacion,
                    FechaActualizacion = x.FechaActualizacion,
                    UsuarioModificacion = x.UsuarioModificacion
                })
                .FirstOrDefaultAsync(ct);
        }

        // ==========================================
        // USUARIOS DISPONIBLES
        // ==========================================

        public async Task<List<UsuarioDisponibleSoporteDto>>
            GetDisponiblesAsync(
                Guid empresaId,
                Guid equipoId,
                string? buscar = null,
                CancellationToken ct = default)
        {
            // Solo usuarios activos de la empresa que
            // no estén registrados en este equipo.
            // Los integrantes inactivos se reactivan,
            // no se insertan nuevamente.

            var query = context.UsuarioEmpresas
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.Activo == true &&
                    x.Usuario.Activo == true &&
                    !context.EquipoSoporteIntegrantes.Any(i =>
                        i.EmpresaId == empresaId &&
                        i.EquipoSoporteId == equipoId &&
                        i.UsuarioId == x.UsuarioId));

            buscar = buscar?.Trim();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                query = query.Where(x =>
                    x.Usuario.Nombre.Contains(buscar) ||
                    x.Usuario.Apellidos.Contains(buscar) ||
                    x.Usuario.Email.Contains(buscar));
            }

            return await query
                .OrderBy(x => x.Usuario.Nombre)
                .ThenBy(x => x.Usuario.Apellidos)
                .Select(x => new UsuarioDisponibleSoporteDto
                {
                    Id = x.UsuarioId,

                    Nombre = x.Usuario.Nombre,
                    Apellidos = x.Usuario.Apellidos,

                    NombreCompleto =
                        x.Usuario.Nombre + " " +
                        x.Usuario.Apellidos,

                    Correo = x.Usuario.Email
                })
                .ToListAsync(ct);
        }

        // ==========================================
        // CREAR
        // ==========================================

        public async Task<ServiceResult<EquipoSoporteIntegranteDto>>
            CreateAsync(
                Guid empresaId,
                Guid equipoId,
                EquipoSoporteIntegranteCrearDto dto,
                Guid usuarioCreacion,
                CancellationToken ct = default)
        {
            if (!dto.UsuarioId.HasValue ||
                dto.UsuarioId.Value == Guid.Empty)
            {
                return ServiceResult<EquipoSoporteIntegranteDto>
                    .Fail("Selecciona un usuario válido.");
            }

            // VALIDAR EQUIPO

            var equipo = await context.EquipoSoportes
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == equipoId &&
                    x.EmpresaId == empresaId,
                    ct);

            if (equipo == null)
            {
                return ServiceResult<EquipoSoporteIntegranteDto>
                    .Fail("Equipo de soporte no encontrado.");
            }

            if (equipo.Activo != true)
            {
                return ServiceResult<EquipoSoporteIntegranteDto>
                    .Fail("No puedes agregar integrantes a un equipo inactivo.");
            }

            // VALIDAR USUARIO Y EMPRESA

            var usuarioValido = await context.UsuarioEmpresas
                .AsNoTracking()
                .AnyAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.UsuarioId == dto.UsuarioId.Value &&
                    x.Activo == true &&
                    x.Usuario.Activo == true,
                    ct);

            if (!usuarioValido)
            {
                return ServiceResult<EquipoSoporteIntegranteDto>
                    .Fail(
                        "El usuario no existe, está inactivo o no pertenece a la empresa."
                    );
            }

            // VALIDAR DUPLICADO

            var existe = await context.EquipoSoporteIntegrantes
                .AsNoTracking()
                .AnyAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.EquipoSoporteId == equipoId &&
                    x.UsuarioId == dto.UsuarioId.Value,
                    ct);

            if (existe)
            {
                return ServiceResult<EquipoSoporteIntegranteDto>
                    .Fail(
                        "El usuario ya está registrado en este equipo. " +
                        "Si está inactivo, puedes reactivarlo."
                    );
            }

            // CREAR

            var integrante = new EquipoSoporteIntegrante
            {
                EmpresaId = empresaId,
                EquipoSoporteId = equipoId,
                UsuarioId = dto.UsuarioId.Value,

                EsLider = false,
                Activo = true,

                FechaCreacion = DateTime.UtcNow,
                UsuarioCreacion = usuarioCreacion
            };

            context.EquipoSoporteIntegrantes.Add(integrante);

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (EsDuplicado(ex))
            {
                return ServiceResult<EquipoSoporteIntegranteDto>
                    .Fail(
                        "El usuario ya está registrado en este equipo."
                    );
            }

            var detalle = await GetByIdAsync(
                empresaId,
                equipoId,
                integrante.Id,
                ct);

            return ServiceResult<EquipoSoporteIntegranteDto>
                .Ok(detalle!);
        }

        // ==========================================
        // ACTIVAR / DESACTIVAR
        // ==========================================

        public async Task<ServiceResult<bool>> UpdateStatusAsync(
            Guid empresaId,
            Guid equipoId,
            Guid id,
            bool activo,
            Guid usuarioModificacion,
            CancellationToken ct = default)
        {
            var integrante = await context.EquipoSoporteIntegrantes
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId &&
                    x.EquipoSoporteId == equipoId,
                    ct);

            if (integrante == null)
            {
                return ServiceResult<bool>
                    .Fail("Integrante no encontrado.");
            }

            if (integrante.Activo == activo)
            {
                return ServiceResult<bool>.Ok(true);
            }

            // ======================================
            // ACTIVAR
            // ======================================

            if (activo)
            {
                var equipoActivo = await context.EquipoSoportes
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == equipoId &&
                        x.EmpresaId == empresaId &&
                        x.Activo == true,
                        ct);

                if (!equipoActivo)
                {
                    return ServiceResult<bool>
                        .Fail(
                            "No puedes activar un integrante de un equipo inactivo."
                        );
                }

                var usuarioValido = await context.UsuarioEmpresas
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.UsuarioId == integrante.UsuarioId &&
                        x.EmpresaId == empresaId &&
                        x.Activo == true &&
                        x.Usuario.Activo == true,
                        ct);

                if (!usuarioValido)
                {
                    return ServiceResult<bool>
                        .Fail(
                            "No puedes activar al integrante porque su usuario " +
                            "está inactivo o ya no pertenece a la empresa."
                        );
                }
            }

            // ======================================
            // DESACTIVAR
            // ======================================

            if (!activo)
            {
                // Bloquear la baja si tiene tickets
                // actualmente asignados como técnico.

                var tieneTicketsVigentes =
                    await context.TicketAsignacions
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.EmpresaId == empresaId &&
                            x.EquipoSoporteId == equipoId &&
                            x.UsuarioTecnicoId == integrante.UsuarioId &&
                            x.FechaFin == null,
                            ct);

                if (tieneTicketsVigentes)
                {
                    return ServiceResult<bool>
                        .Fail(
                            "No puedes desactivar al integrante porque tiene " +
                            "tickets asignados actualmente."
                        );
                }
            }

            // GUARDAR ESTATUS

            integrante.Activo = activo;

            integrante.FechaActualizacion = DateTime.UtcNow;
            integrante.UsuarioModificacion = usuarioModificacion;

            await context.SaveChangesAsync(ct);

            return ServiceResult<bool>.Ok(true);
        }

        // ==========================================
        // HELPERS
        // ==========================================

        private static bool EsDuplicado(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sql &&
                   (sql.Number == 2601 || sql.Number == 2627);
        }
    }
}
