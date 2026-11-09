using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto.Responsables;
using Nextag.Api.Models.Dto;
using Nextag.Api.Services.Responsables.Interface;

namespace Nextag.Api.Services.Responsables.Implementation
{
    public class ResponsableService : IResponsableService
    {
        private readonly NextagContext context;

        public ResponsableService(NextagContext context)
        {
            this.context = context;
        }

        // ==========================================
        // CONSULTA BASE
        // ==========================================

        private IQueryable<ResponsableDto> Consulta(
            Guid empresaId)
        {
            return context.Responsables
                .AsNoTracking()
                .Where(x => x.EmpresaId == empresaId)
                .Select(x => new ResponsableDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,
                    Tipo = x.Tipo,
                    Nombre = x.Nombre,
                    Codigo = x.Codigo,

                    Puesto = x.Puesto,
                    Departamento = x.Departamento,

                    SucursalId = x.SucursalId,

                    SucursalNombre = x.Sucursal != null
                        ? x.Sucursal.Nombre
                        : null,

                    AreaResponsableId = x.AreaResponsableId,

                    AreaResponsableNombre =
                        x.ResponsableNavigation != null
                            ? x.ResponsableNavigation.Nombre
                            : null,

                    Contacto = x.Contacto,
                    Correo = x.Correo,
                    Telefono = x.Telefono,
                    Observaciones = x.Observaciones,

                    Activo = x.Activo == true,

                    TotalPersonas = x.Tipo == "AREA"
                        ? x.InverseResponsableNavigation
                            .Count(p =>
                                p.Tipo == "PERSONA" &&
                                p.Activo == true)
                        : 0,

                    FechaCreacion = x.FechaCreacion,
                    UsuarioCreacion = x.UsuarioCreacion,

                    FechaActualizacion = x.FechaActualizacion,
                    UsuarioModificacion = x.UsuarioModificacion
                });
        }

        // ==========================================
        // LISTADO
        // ==========================================

        public async Task<ResponsablePaginaDto> GetAllAsync(
            Guid empresaId,
            ResponsableFiltroDto filtro,
            CancellationToken ct = default)
        {
            var pagina = Math.Max(1, filtro.Pagina);

            var tamano = Math.Clamp(
                filtro.TamanoPagina,
                1,
                100);

            var query = Consulta(empresaId);

            if (!string.IsNullOrWhiteSpace(filtro.Tipo))
            {
                var tipo = filtro.Tipo.Trim().ToUpperInvariant();

                query = query.Where(x => x.Tipo == tipo);
            }

            if (filtro.Activo.HasValue)
            {
                query = query.Where(
                    x => x.Activo == filtro.Activo.Value);
            }

            if (filtro.SucursalId.HasValue)
            {
                query = query.Where(
                    x => x.SucursalId == filtro.SucursalId);
            }

            if (filtro.AreaResponsableId.HasValue)
            {
                query = query.Where(
                    x => x.AreaResponsableId ==
                         filtro.AreaResponsableId);
            }

            if (!string.IsNullOrWhiteSpace(filtro.Buscar))
            {
                var buscar = filtro.Buscar.Trim();

                query = query.Where(x =>
                    x.Nombre.Contains(buscar) ||
                    (x.Codigo != null &&
                     x.Codigo.Contains(buscar)));
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderBy(x => x.Nombre)
                .ThenBy(x => x.Id)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync(ct);

            return new ResponsablePaginaDto
            {
                Items = items,
                Total = total,
                Pagina = pagina,
                TamanoPagina = tamano,

                TotalPaginas = (int)Math.Ceiling(
                    total / (double)tamano)
            };
        }

        // ==========================================
        // DETALLE
        // ==========================================

        public async Task<ResponsableDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default)
        {
            return await Consulta(empresaId)
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    ct);
        }

        // ==========================================
        // CREAR
        // ==========================================

        public async Task<ServiceResult<ResponsableDto>>
            CreateAsync(
                ResponsableGuardarDto dto,
                Guid empresaId,
                Guid usuarioId,
                CancellationToken ct = default)
        {
            var tipo = dto.Tipo?.Trim().ToUpperInvariant();
            var nombre = dto.Nombre?.Trim();
            var codigo = NormalizarCodigo(dto.Codigo);

            var validacion = await ValidarAsync(
                dto,
                empresaId,
                null,
                tipo,
                codigo,
                ct);

            if (validacion != null)
            {
                return ServiceResult<ResponsableDto>
                    .Fail(validacion);
            }

            var responsable = new Responsable
            {
                EmpresaId = empresaId,

                Tipo = tipo!,
                Nombre = nombre!,
                Codigo = codigo,

                Puesto = tipo == "PERSONA"
                    ? Normalizar(dto.Puesto)
                    : null,

                Departamento = tipo == "PERSONA"
                    ? Normalizar(dto.Departamento)
                    : null,

                SucursalId = dto.SucursalId,

                AreaResponsableId = tipo == "PERSONA"
                    ? dto.AreaResponsableId
                    : null,

                Contacto = tipo == "AREA"
                    ? Normalizar(dto.Contacto)
                    : null,

                Correo = Normalizar(dto.Correo),
                Telefono = Normalizar(dto.Telefono),

                Observaciones = Normalizar(
                    dto.Observaciones),

                Activo = true,

                UsuarioCreacion = usuarioId
            };

            context.Responsables.Add(responsable);

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
                when (EsDuplicado(ex))
            {
                return ServiceResult<ResponsableDto>
                    .Fail(
                        "Ya existe un responsable con ese código " +
                        "para el mismo tipo y empresa.");
            }

            var detalle = await GetByIdAsync(
                responsable.Id,
                empresaId,
                ct);

            return ServiceResult<ResponsableDto>
                .Ok(detalle!);
        }

        // ==========================================
        // ACTUALIZAR
        // ==========================================

        public async Task<ServiceResult<ResponsableDto>>
            UpdateAsync(
                Guid id,
                ResponsableGuardarDto dto,
                Guid empresaId,
                Guid usuarioId,
                CancellationToken ct = default)
        {
            var responsable = await context.Responsables
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId,
                    ct);

            if (responsable == null)
            {
                return ServiceResult<ResponsableDto>
                    .Fail("Responsable no encontrado.");
            }

            var tipo = dto.Tipo?.Trim().ToUpperInvariant();

            if (responsable.Tipo != tipo)
            {
                return ServiceResult<ResponsableDto>
                    .Fail(
                        "No se permite cambiar el tipo " +
                        "de un responsable.");
            }

            var codigo = NormalizarCodigo(dto.Codigo);

            var validacion = await ValidarAsync(
                dto,
                empresaId,
                id,
                tipo,
                codigo,
                ct);

            if (validacion != null)
            {
                return ServiceResult<ResponsableDto>
                    .Fail(validacion);
            }

            responsable.Nombre = dto.Nombre.Trim();
            responsable.Codigo = codigo;

            responsable.Puesto = tipo == "PERSONA"
                ? Normalizar(dto.Puesto)
                : null;

            responsable.Departamento = tipo == "PERSONA"
                ? Normalizar(dto.Departamento)
                : null;

            responsable.SucursalId = dto.SucursalId;

            responsable.AreaResponsableId =
                tipo == "PERSONA"
                    ? dto.AreaResponsableId
                    : null;

            responsable.Contacto = tipo == "AREA"
                ? Normalizar(dto.Contacto)
                : null;

            responsable.Correo = Normalizar(dto.Correo);
            responsable.Telefono = Normalizar(dto.Telefono);

            responsable.Observaciones =
                Normalizar(dto.Observaciones);

            responsable.FechaActualizacion = DateTime.UtcNow;
            responsable.UsuarioModificacion = usuarioId;

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
                when (EsDuplicado(ex))
            {
                return ServiceResult<ResponsableDto>
                    .Fail(
                        "Ya existe un responsable con ese código " +
                        "para el mismo tipo y empresa.");
            }

            var detalle = await GetByIdAsync(
                id,
                empresaId,
                ct);

            return ServiceResult<ResponsableDto>
                .Ok(detalle!);
        }

        // ==========================================
        // ACTIVAR / DESACTIVAR
        // ==========================================

        public async Task<ServiceResult<bool>>
            UpdateStatusAsync(
                Guid id,
                Guid empresaId,
                bool activo,
                Guid usuarioId,
                CancellationToken ct = default)
        {
            var responsable = await context.Responsables
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId,
                    ct);

            if (responsable == null)
            {
                return ServiceResult<bool>
                    .Fail("Responsable no encontrado.");
            }

            if (responsable.Activo == activo)
            {
                return ServiceResult<bool>.Ok(true);
            }

            if (!activo && responsable.Tipo == "AREA")
            {
                var tienePersonas = await context.Responsables
                    .AnyAsync(x =>
                        x.EmpresaId == empresaId &&
                        x.AreaResponsableId == id &&
                        x.Activo == true,
                        ct);

                if (tienePersonas)
                {
                    return ServiceResult<bool>.Fail(
                        "No se puede desactivar el área " +
                        "porque tiene personas activas asociadas.");
                }
            }

            // Cuando exista ActivoAsignacion, aquí también
            // validaremos asignaciones vigentes.

            responsable.Activo = activo;
            responsable.FechaActualizacion = DateTime.UtcNow;
            responsable.UsuarioModificacion = usuarioId;

            await context.SaveChangesAsync(ct);

            return ServiceResult<bool>.Ok(true);
        }

        // ==========================================
        // VALIDACIONES
        // ==========================================

        private async Task<string?> ValidarAsync(
            ResponsableGuardarDto dto,
            Guid empresaId,
            Guid? idActual,
            string? tipo,
            string? codigo,
            CancellationToken ct)
        {
            if (tipo != "PERSONA" && tipo != "AREA")
            {
                return "El tipo debe ser PERSONA o AREA.";
            }

            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return "El nombre del responsable es obligatorio.";
            }

            if (dto.Nombre.Trim().Length > 200)
            {
                return "El nombre no puede superar 200 caracteres.";
            }

            if (codigo != null)
            {
                var duplicado = await context.Responsables
                    .AnyAsync(x =>
                        x.EmpresaId == empresaId &&
                        x.Tipo == tipo &&
                        x.Codigo == codigo &&
                        (!idActual.HasValue ||
                         x.Id != idActual.Value),
                        ct);

                if (duplicado)
                {
                    return "Ya existe un responsable con " +
                           "ese código para el mismo tipo.";
                }
            }

            // Validar sucursal.

            if (dto.SucursalId.HasValue)
            {
                var sucursalValida = await context.Sucursals
                    .AnyAsync(x =>
                        x.Id == dto.SucursalId.Value &&
                        x.EmpresaId == empresaId &&
                        x.Activo == true,
                        ct);

                if (!sucursalValida)
                {
                    return "La sucursal no existe, está " +
                           "inactiva o no pertenece a la empresa.";
                }
            }

            // Validar área relacionada.

            if (tipo == "AREA" &&
                dto.AreaResponsableId.HasValue)
            {
                return "Un área no puede pertenecer a otra área.";
            }

            if (dto.AreaResponsableId.HasValue)
            {
                if (idActual == dto.AreaResponsableId)
                {
                    return "Un responsable no puede " +
                           "relacionarse consigo mismo.";
                }

                var areaValida = await context.Responsables
                    .AnyAsync(x =>
                        x.Id == dto.AreaResponsableId.Value &&
                        x.EmpresaId == empresaId &&
                        x.Tipo == "AREA" &&
                        x.Activo == true,
                        ct);

                if (!areaValida)
                {
                    return "El área seleccionada no existe, " +
                           "está inactiva o pertenece a otra empresa.";
                }
            }

            return null;
        }

        // ==========================================
        // HELPERS
        // ==========================================

        private static string? Normalizar(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string? NormalizarCodigo(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToUpperInvariant();
        }

        private static bool EsDuplicado(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sqlEx &&
                   (sqlEx.Number == 2601 ||
                    sqlEx.Number == 2627);
        }
    }
}
