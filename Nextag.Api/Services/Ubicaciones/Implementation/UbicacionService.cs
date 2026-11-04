using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto;
using Nextag.Api.Models.Dto.Ubicaciones;
using Nextag.Api.Services.Ubicaciones.Interface;

namespace Nextag.Api.Services.Ubicaciones.Implementation
{
    public class UbicacionService : IUbicacionService
    {
        private readonly NextagContext context;

        public UbicacionService(NextagContext context)
        {
            this.context = context;
        }

        public async Task<List<UbicacionListDto>> GetAllAsync(
    Guid empresaId,
    Guid? sucursalId = null,
    bool? activo = null,
    CancellationToken ct = default)
        {
            var query = context.Ubicacions
                .AsNoTracking()
                .Where(x => x.EmpresaId == empresaId);

            if (sucursalId.HasValue)
            {
                query = query.Where(x =>
                    x.SucursalId == sucursalId.Value);
            }

            if (activo.HasValue)
            {
                query = query.Where(x =>
                    x.Activo == activo.Value);
            }

            return await query
                .OrderBy(x => x.Sucursal.Nombre)
                .ThenBy(x => x.Nombre)
                .Select(x => new UbicacionListDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,

                    SucursalId = x.SucursalId,
                    SucursalNombre = x.Sucursal.Nombre,

                    TipoUbicacionId = x.TipoUbicacionId,
                    TipoUbicacionNombre = x.TipoUbicacion != null
                        ? x.TipoUbicacion.Nombre
                        : null,

                    UbicacionPadreId = x.UbicacionPadreId,
                    UbicacionPadreNombre = x.UbicacionNavigation != null
                        ? x.UbicacionNavigation.Nombre
                        : null,

                    Clave = x.Clave,
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion,
                    Activo = x.Activo == true
                })
                .ToListAsync(ct);
        }

        public async Task<UbicacionDetalleDto?> GetByIdAsync(
    Guid id,
    Guid empresaId,
    CancellationToken ct = default)
        {
            return await context.Ubicacions
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId)
                .Select(x => new UbicacionDetalleDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,

                    SucursalId = x.SucursalId,
                    SucursalNombre = x.Sucursal.Nombre,

                    TipoUbicacionId = x.TipoUbicacionId,
                    TipoUbicacionNombre = x.TipoUbicacion != null
                        ? x.TipoUbicacion.Nombre
                        : null,

                    UbicacionPadreId = x.UbicacionPadreId,
                    UbicacionPadreNombre = x.UbicacionNavigation != null
                        ? x.UbicacionNavigation.Nombre
                        : null,

                    Clave = x.Clave,
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion,
                    Activo = x.Activo == true,

                    FechaCreacion = x.FechaCreacion,
                    UsuarioCreacion = x.UsuarioCreacion,
                    FechaActualizacion = x.FechaActualizacion,
                    UsuarioModificacion = x.UsuarioModificacion
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<ServiceResult<bool>> UpdateStatusAsync(
    Guid id,
    Guid empresaId,
    bool activo,
    Guid usuarioModificacion,
    CancellationToken ct = default)
        {
            var ubicacion = await context.Ubicacions
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId,
                    ct);

            if (ubicacion == null)
            {
                return ServiceResult<bool>
                    .Fail("Ubicación no encontrada.");
            }

            if (ubicacion.Activo == activo)
            {
                return ServiceResult<bool>.Ok(true);
            }

            if (!activo)
            {
                // No desactivar si tiene ubicaciones hijas activas
                var tieneHijosActivos = await context.Ubicacions
                    .AnyAsync(x =>
                        x.EmpresaId == empresaId &&
                        x.SucursalId == ubicacion.SucursalId &&
                        x.UbicacionPadreId == id &&
                        x.Activo == true,
                        ct);

                if (tieneHijosActivos)
                {
                    return ServiceResult<bool>
                        .Fail("No puedes desactivar una ubicación que tiene ubicaciones hijas activas.");
                }
            }
            else
            {
                // Validar sucursal antes de reactivar
                var errorSucursal = await ValidarSucursalAsync(
                    empresaId,
                    ubicacion.SucursalId,
                    ct);

                if (errorSucursal != null)
                {
                    return ServiceResult<bool>
                        .Fail(errorSucursal);
                }

                // Validar tipo antes de reactivar
                var errorTipo = await ValidarTipoUbicacionAsync(
                    empresaId,
                    ubicacion.TipoUbicacionId,
                    true,
                    ct);

                if (errorTipo != null)
                {
                    return ServiceResult<bool>
                        .Fail(errorTipo);
                }

                // Validar jerarquía antes de reactivar
                var errorJerarquia = await ValidarJerarquiaAsync(
                    empresaId,
                    ubicacion.SucursalId,
                    ubicacion.UbicacionPadreId,
                    id,
                    ct);

                if (errorJerarquia != null)
                {
                    return ServiceResult<bool>
                        .Fail(errorJerarquia);
                }
            }

            ubicacion.Activo = activo;
            ubicacion.FechaActualizacion = DateTime.UtcNow;
            ubicacion.UsuarioModificacion = usuarioModificacion;

            await context.SaveChangesAsync(ct);

            return ServiceResult<bool>.Ok(true);
        }
        public async Task<ServiceResult<UbicacionDetalleDto>> UpdateAsync(
    Guid id,
    UbicacionUpdateDto dto,
    Guid empresaId,
    Guid usuarioModificacion,
    CancellationToken ct = default)
        {
            var ubicacion = await context.Ubicacions
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId,
                    ct);

            if (ubicacion == null)
            {
                return ServiceResult<UbicacionDetalleDto>
                    .Fail("Ubicación no encontrada.");
            }

            // No permitir cambiar de sucursal
            if (ubicacion.SucursalId != dto.SucursalId)
            {
                return ServiceResult<UbicacionDetalleDto>
                    .Fail("No se permite cambiar una ubicación de sucursal.");
            }

            var clave = dto.Clave?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(clave))
            {
                return ServiceResult<UbicacionDetalleDto>
                    .Fail("La clave de la ubicación es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return ServiceResult<UbicacionDetalleDto>
                    .Fail("El nombre de la ubicación es obligatorio.");
            }

            // Validar sucursal
            var errorSucursal = await ValidarSucursalAsync(
                empresaId,
                dto.SucursalId,
                ct);

            if (errorSucursal != null)
            {
                return ServiceResult<UbicacionDetalleDto>
                    .Fail(errorSucursal);
            }

            // Si cambia el tipo, el nuevo debe estar activo.
            // Si conserva uno que después fue desactivado,
            // permitimos editar sus demás datos.
            var cambiaTipo = ubicacion.TipoUbicacionId != dto.TipoUbicacionId;

            var errorTipo = await ValidarTipoUbicacionAsync(
                empresaId,
                dto.TipoUbicacionId,
                cambiaTipo,
                ct);

            if (errorTipo != null)
            {
                return ServiceResult<UbicacionDetalleDto>
                    .Fail(errorTipo);
            }

            // Validar nueva jerarquía
            var errorJerarquia = await ValidarJerarquiaAsync(
                empresaId,
                dto.SucursalId,
                dto.UbicacionPadreId,
                id,
                ct);

            if (errorJerarquia != null)
            {
                return ServiceResult<UbicacionDetalleDto>
                    .Fail(errorJerarquia);
            }

            // Clave única dentro de la sucursal
            var existe = await context.Ubicacions
                .AnyAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.SucursalId == dto.SucursalId &&
                    x.Clave == clave &&
                    x.Id != id,
                    ct);

            if (existe)
            {
                return ServiceResult<UbicacionDetalleDto>
                    .Fail("Ya existe otra ubicación con esa clave en la sucursal.");
            }

            ubicacion.Clave = clave;
            ubicacion.Nombre = dto.Nombre.Trim();
            ubicacion.Descripcion = Normalizar(dto.Descripcion);

            ubicacion.TipoUbicacionId = dto.TipoUbicacionId;
            ubicacion.UbicacionPadreId = dto.UbicacionPadreId;

            ubicacion.FechaActualizacion = DateTime.UtcNow;
            ubicacion.UsuarioModificacion = usuarioModificacion;

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (EsClaveDuplicada(ex))
            {
                return ServiceResult<UbicacionDetalleDto>
                    .Fail("Ya existe otra ubicación con esa clave en la sucursal.");
            }

            var detalle = await GetByIdAsync(
                id,
                empresaId,
                ct);

            return ServiceResult<UbicacionDetalleDto>.Ok(detalle!);
        }

        public async Task<ServiceResult<UbicacionDetalleDto>> CreateAsync(
    UbicacionCreateDto dto,
    Guid empresaId,
    Guid usuarioCreacion,
    CancellationToken ct = default)
        {
            var clave = dto.Clave?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(clave))
            {
                return ServiceResult<UbicacionDetalleDto>
                    .Fail("La clave de la ubicación es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return ServiceResult<UbicacionDetalleDto>
                    .Fail("El nombre de la ubicación es obligatorio.");
            }

            // Validar sucursal
            var errorSucursal = await ValidarSucursalAsync(
                empresaId,
                dto.SucursalId,
                ct);

            if (errorSucursal != null)
            {
                return ServiceResult<UbicacionDetalleDto>
                    .Fail(errorSucursal);
            }

            // Validar tipo de ubicación
            var errorTipo = await ValidarTipoUbicacionAsync(
                empresaId,
                dto.TipoUbicacionId,
                true,
                ct);

            if (errorTipo != null)
            {
                return ServiceResult<UbicacionDetalleDto>
                    .Fail(errorTipo);
            }

            // Validar jerarquía
            var errorJerarquia = await ValidarJerarquiaAsync(
                empresaId,
                dto.SucursalId,
                dto.UbicacionPadreId,
                null,
                ct);

            if (errorJerarquia != null)
            {
                return ServiceResult<UbicacionDetalleDto>
                    .Fail(errorJerarquia);
            }

            // Validar clave única por sucursal
            var existe = await context.Ubicacions
                .AnyAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.SucursalId == dto.SucursalId &&
                    x.Clave == clave,
                    ct);

            if (existe)
            {
                return ServiceResult<UbicacionDetalleDto>
                    .Fail("Ya existe una ubicación con esa clave en la sucursal.");
            }

            var ubicacion = new Ubicacion
            {
                EmpresaId = empresaId,
                SucursalId = dto.SucursalId,
                TipoUbicacionId = dto.TipoUbicacionId,
                UbicacionPadreId = dto.UbicacionPadreId,

                Clave = clave,
                Nombre = dto.Nombre.Trim(),
                Descripcion = Normalizar(dto.Descripcion),

                Activo = true,
                UsuarioCreacion = usuarioCreacion
            };

            context.Ubicacions.Add(ubicacion);

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (EsClaveDuplicada(ex))
            {
                return ServiceResult<UbicacionDetalleDto>
                    .Fail("Ya existe una ubicación con esa clave en la sucursal.");
            }

            var detalle = await GetByIdAsync(
                ubicacion.Id,
                empresaId,
                ct);

            return ServiceResult<UbicacionDetalleDto>.Ok(detalle!);
        }

        private async Task<string?> ValidarSucursalAsync(
    Guid empresaId,
    Guid sucursalId,
    CancellationToken ct)
        {
            if (sucursalId == Guid.Empty)
                return "Debes seleccionar una sucursal.";

            var sucursal = await context.Sucursals
                .AsNoTracking()
                .Where(x =>
                    x.Id == sucursalId &&
                    x.EmpresaId == empresaId)
                .Select(x => new
                {
                    x.Activo
                })
                .FirstOrDefaultAsync(ct);

            if (sucursal == null)
                return "La sucursal seleccionada no existe en esta empresa.";

            if (sucursal.Activo != true)
                return "La sucursal seleccionada está inactiva.";

            return null;
        }

        private async Task<string?> ValidarTipoUbicacionAsync(
    Guid empresaId,
    Guid? tipoUbicacionId,
    bool exigirActivo,
    CancellationToken ct)
        {
            if (!tipoUbicacionId.HasValue)
                return null;

            if (tipoUbicacionId.Value == Guid.Empty)
                return "El tipo de ubicación no es válido.";

            var tipo = await context.TipoUbicacions
                .AsNoTracking()
                .Where(x =>
                    x.Id == tipoUbicacionId.Value &&
                    x.EmpresaId == empresaId)
                .Select(x => new
                {
                    x.Activo
                })
                .FirstOrDefaultAsync(ct);

            if (tipo == null)
                return "El tipo de ubicación no pertenece a esta empresa.";

            if (exigirActivo && tipo.Activo != true)
                return "El tipo de ubicación seleccionado está inactivo.";

            return null;
        }

        private async Task<string?> ValidarJerarquiaAsync(
    Guid empresaId,
    Guid sucursalId,
    Guid? ubicacionPadreId,
    Guid? ubicacionActualId,
    CancellationToken ct)
        {
            if (!ubicacionPadreId.HasValue)
                return null;

            var visitados = new HashSet<Guid>();

            if (ubicacionActualId.HasValue)
            {
                visitados.Add(ubicacionActualId.Value);
            }

            Guid? actualId = ubicacionPadreId;

            while (actualId.HasValue)
            {
                if (!visitados.Add(actualId.Value))
                {
                    return "La ubicación padre genera una relación circular.";
                }

                var padre = await context.Ubicacions
                    .AsNoTracking()
                    .Where(x =>
                        x.Id == actualId.Value &&
                        x.EmpresaId == empresaId &&
                        x.SucursalId == sucursalId)
                    .Select(x => new
                    {
                        x.UbicacionPadreId,
                        x.Activo
                    })
                    .FirstOrDefaultAsync(ct);

                if (padre == null)
                {
                    return "La ubicación padre no existe o pertenece a otra sucursal.";
                }

                if (padre.Activo != true)
                {
                    return "La ubicación padre o alguno de sus niveles superiores está inactivo.";
                }

                actualId = padre.UbicacionPadreId;
            }

            return null;
        }

        private static string? Normalizar(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static bool EsClaveDuplicada(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sqlEx &&
                (sqlEx.Number == 2601 || sqlEx.Number == 2627);
        }
    }
}
