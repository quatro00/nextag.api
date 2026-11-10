using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto.EquiposSoporte;
using Nextag.Api.Models.Dto;
using Nextag.Api.Services.EquiposSoporte.Interface;

namespace Nextag.Api.Services.EquiposSoporte.Implementation
{
    public class EquipoSoporteService : IEquipoSoporteService
    {
        private readonly NextagContext context;

        public EquipoSoporteService(NextagContext context)
        {
            this.context = context;
        }

        // ==========================================
        // LISTADO
        // ==========================================

        public async Task<List<EquipoSoporteListDto>> GetAllAsync(
            Guid empresaId,
            bool? activo = null,
            string? tipo = null,
            string? buscar = null,
            CancellationToken ct = default)
        {
            var query = context.EquipoSoportes
                .AsNoTracking()
                .Where(x => x.EmpresaId == empresaId);

            // FILTRO ESTATUS

            if (activo.HasValue)
            {
                query = query.Where(x =>
                    x.Activo == activo.Value);
            }

            // FILTRO TIPO

            tipo = tipo?.Trim().ToUpperInvariant();

            if (!string.IsNullOrWhiteSpace(tipo))
            {
                query = query.Where(x =>
                    x.Tipo == tipo);
            }

            // BUSQUEDA

            buscar = buscar?.Trim();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                query = query.Where(x =>
                    x.Clave.Contains(buscar) ||
                    x.Nombre.Contains(buscar) ||
                    (x.Descripcion != null &&
                     x.Descripcion.Contains(buscar)));
            }

            // CONSULTA

            return await query
                .OrderBy(x => x.Nombre)
                .Select(x => new EquipoSoporteListDto
                {
                    Id = x.Id,

                    EmpresaId = x.EmpresaId,

                    Clave = x.Clave,
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion,

                    Tipo = x.Tipo,

                    ProveedorId = x.ProveedorId,

                    ProveedorNombre = x.Proveedor != null
                        ? x.Proveedor.Nombre
                        : null,

                    CorreoContacto = x.CorreoContacto,

                    Activo = x.Activo == true,

                    TotalIntegrantes =
                        x.EquipoSoporteIntegrantes.Count(i =>
                            i.Activo == true)
                })
                .ToListAsync(ct);
        }

        // ==========================================
        // DETALLE
        // ==========================================

        public async Task<EquipoSoporteDetalleDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default)
        {
            return await context.EquipoSoportes
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId)
                .Select(x => new EquipoSoporteDetalleDto
                {
                    Id = x.Id,

                    EmpresaId = x.EmpresaId,

                    Clave = x.Clave,
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion,

                    Tipo = x.Tipo,

                    ProveedorId = x.ProveedorId,

                    ProveedorNombre = x.Proveedor != null
                        ? x.Proveedor.Nombre
                        : null,

                    CorreoContacto = x.CorreoContacto,

                    Activo = x.Activo == true,

                    TotalIntegrantes =
                        x.EquipoSoporteIntegrantes.Count(i =>
                            i.Activo == true),

                    FechaCreacion = x.FechaCreacion,

                    UsuarioCreacion = x.UsuarioCreacion,

                    FechaActualizacion = x.FechaActualizacion,

                    UsuarioModificacion = x.UsuarioModificacion
                })
                .FirstOrDefaultAsync(ct);
        }

        // ==========================================
        // CREAR
        // ==========================================

        public async Task<ServiceResult<EquipoSoporteDetalleDto>> CreateAsync(
            EquipoSoporteGuardarDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default)
        {
            var error = await ValidarDatosAsync(
                dto,
                empresaId,
                null,
                ct);

            if (error != null)
            {
                return ServiceResult<EquipoSoporteDetalleDto>
                    .Fail(error);
            }

            var equipo = new EquipoSoporte
            {
                EmpresaId = empresaId,

                Clave = dto.Clave.Trim().ToUpperInvariant(),

                Nombre = dto.Nombre.Trim(),

                Descripcion = Normalizar(dto.Descripcion),

                Tipo = dto.Tipo.Trim().ToUpperInvariant(),

                ProveedorId = dto.ProveedorId,

                CorreoContacto = Normalizar(dto.CorreoContacto),

                Activo = true,

                FechaCreacion = DateTime.UtcNow,

                UsuarioCreacion = usuarioId
            };

            context.EquipoSoportes.Add(equipo);

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (EsDuplicado(ex))
            {
                return ServiceResult<EquipoSoporteDetalleDto>
                    .Fail(
                        "Ya existe un equipo de soporte con esa clave en la empresa."
                    );
            }

            var detalle = await GetByIdAsync(
                equipo.Id,
                empresaId,
                ct);

            return ServiceResult<EquipoSoporteDetalleDto>
                .Ok(detalle!);
        }

        // ==========================================
        // ACTUALIZAR
        // ==========================================

        public async Task<ServiceResult<EquipoSoporteDetalleDto>> UpdateAsync(
            Guid id,
            EquipoSoporteGuardarDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default)
        {
            var equipo = await context.EquipoSoportes
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId,
                    ct);

            if (equipo == null)
            {
                return ServiceResult<EquipoSoporteDetalleDto>
                    .Fail("Equipo de soporte no encontrado.");
            }

            var error = await ValidarDatosAsync(
                dto,
                empresaId,
                id,
                ct);

            if (error != null)
            {
                return ServiceResult<EquipoSoporteDetalleDto>
                    .Fail(error);
            }

            equipo.Clave =
                dto.Clave.Trim().ToUpperInvariant();

            equipo.Nombre =
                dto.Nombre.Trim();

            equipo.Descripcion =
                Normalizar(dto.Descripcion);

            equipo.Tipo =
                dto.Tipo.Trim().ToUpperInvariant();

            equipo.ProveedorId =
                dto.ProveedorId;

            equipo.CorreoContacto =
                Normalizar(dto.CorreoContacto);

            equipo.FechaActualizacion =
                DateTime.UtcNow;

            equipo.UsuarioModificacion =
                usuarioId;

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (EsDuplicado(ex))
            {
                return ServiceResult<EquipoSoporteDetalleDto>
                    .Fail(
                        "Ya existe otro equipo de soporte con esa clave en la empresa."
                    );
            }

            var detalle = await GetByIdAsync(
                id,
                empresaId,
                ct);

            return ServiceResult<EquipoSoporteDetalleDto>
                .Ok(detalle!);
        }

        // ==========================================
        // ACTIVAR / DESACTIVAR
        // ==========================================

        public async Task<ServiceResult<bool>> UpdateStatusAsync(
            Guid id,
            Guid empresaId,
            bool activo,
            Guid usuarioId,
            CancellationToken ct = default)
        {
            var equipo = await context.EquipoSoportes
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId,
                    ct);

            if (equipo == null)
            {
                return ServiceResult<bool>
                    .Fail("Equipo de soporte no encontrado.");
            }

            if (equipo.Activo == activo)
            {
                return ServiceResult<bool>.Ok(true);
            }

            // ======================================
            // VALIDAR DESACTIVACION
            // ======================================

            if (!activo)
            {
                var tieneTicketsVigentes =
                    await context.TicketAsignacions
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.EmpresaId == empresaId &&
                            x.EquipoSoporteId == id &&
                            x.FechaFin == null,
                            ct);

                if (tieneTicketsVigentes)
                {
                    return ServiceResult<bool>.Fail(
                        "No puedes desactivar un equipo que tiene tickets asignados actualmente."
                    );
                }

                var tieneOrdenesPendientes =
                    await context.OrdenTrabajos
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.EmpresaId == empresaId &&
                            x.EquipoSoporteId == id &&
                            x.EstadoOrdenTrabajoId != 4 &&
                            x.EstadoOrdenTrabajoId != 5,
                            ct);

                if (tieneOrdenesPendientes)
                {
                    return ServiceResult<bool>.Fail(
                        "No puedes desactivar un equipo que tiene órdenes de trabajo pendientes."
                    );
                }
            }

            // ======================================
            // VALIDAR REACTIVACION
            // ======================================

            if (activo && equipo.ProveedorId.HasValue)
            {
                var proveedorValido =
                    await context.Proveedors
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.Id == equipo.ProveedorId.Value &&
                            x.EmpresaId == empresaId &&
                            x.Activo == true,
                            ct);

                if (!proveedorValido)
                {
                    return ServiceResult<bool>.Fail(
                        "No puedes activar el equipo porque su proveedor está inactivo o no existe."
                    );
                }
            }

            equipo.Activo = activo;

            equipo.FechaActualizacion = DateTime.UtcNow;

            equipo.UsuarioModificacion = usuarioId;

            await context.SaveChangesAsync(ct);

            return ServiceResult<bool>.Ok(true);
        }

        // ==========================================
        // VALIDACIONES
        // ==========================================

        private async Task<string?> ValidarDatosAsync(
            EquipoSoporteGuardarDto dto,
            Guid empresaId,
            Guid? excluirId,
            CancellationToken ct)
        {
            // CLAVE

            var clave = dto.Clave?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(clave))
            {
                return "La clave del equipo es obligatoria.";
            }

            if (clave.Length > 40)
            {
                return "La clave no puede superar 40 caracteres.";
            }

            // NOMBRE

            var nombre = dto.Nombre?.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                return "El nombre del equipo es obligatorio.";
            }

            if (nombre.Length > 150)
            {
                return "El nombre no puede superar 150 caracteres.";
            }

            // DESCRIPCION

            var descripcion = Normalizar(dto.Descripcion);

            if (descripcion?.Length > 500)
            {
                return "La descripción no puede superar 500 caracteres.";
            }

            // TIPO

            var tipo = dto.Tipo?.Trim().ToUpperInvariant();

            if (tipo is not ("INTERNO" or "EXTERNO" or "MIXTO"))
            {
                return "El tipo debe ser INTERNO, EXTERNO o MIXTO.";
            }

            // CORREO

            var correo = Normalizar(dto.CorreoContacto);

            if (correo?.Length > 200)
            {
                return "El correo de contacto no puede superar 200 caracteres.";
            }

            // VALIDAR PROVEEDOR SEGUN TIPO

            if (tipo == "INTERNO" && dto.ProveedorId.HasValue)
            {
                return "Un equipo interno no debe tener un proveedor relacionado.";
            }

            if (tipo is "EXTERNO" or "MIXTO")
            {
                if (!dto.ProveedorId.HasValue ||
                    dto.ProveedorId.Value == Guid.Empty)
                {
                    return "Selecciona un proveedor para los equipos externos o mixtos.";
                }
            }

            // VALIDAR PROVEEDOR EXISTENTE Y ACTIVO

            if (dto.ProveedorId.HasValue)
            {
                var proveedorValido =
                    await context.Proveedors
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.Id == dto.ProveedorId.Value &&
                            x.EmpresaId == empresaId &&
                            x.Activo == true,
                            ct);

                if (!proveedorValido)
                {
                    return "El proveedor no existe, está inactivo o pertenece a otra empresa.";
                }
            }

            // CLAVE UNICA POR EMPRESA

            var existe = await context.EquipoSoportes
                .AsNoTracking()
                .AnyAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.Clave == clave &&
                    (!excluirId.HasValue || x.Id != excluirId.Value),
                    ct);

            if (existe)
            {
                return "Ya existe un equipo de soporte con esa clave en la empresa.";
            }

            return null;
        }

        // ==========================================
        // HELPERS
        // ==========================================

        private static string? Normalizar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? null
                : valor.Trim();
        }

        private static bool EsDuplicado(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sql &&
                   (sql.Number == 2601 || sql.Number == 2627);
        }
    }
}
