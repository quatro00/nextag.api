using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto.Sucursales;
using Nextag.Api.Models.Dto;
using Nextag.Api.Services.Sucursales.Interface;

namespace Nextag.Api.Services.Sucursales.Implementation
{
    public class SucursalService : ISucursalService
    {
        private readonly NextagContext context;

        public SucursalService(NextagContext context)
        {
            this.context = context;
        }

        // ==========================================
        // LISTADO
        // ==========================================

        public async Task<List<SucursalListDto>> GetAllAsync(
            Guid empresaId,
            bool? activo = null,
            CancellationToken ct = default)
        {
            var query = context.Sucursals
                .AsNoTracking()
                .Where(x => x.EmpresaId == empresaId);

            if (activo.HasValue)
            {
                query = query.Where(x => x.Activo == activo.Value);
            }

            return await query
                .OrderBy(x => x.Nombre)
                .Select(x => new SucursalListDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,
                    Clave = x.Clave,
                    Nombre = x.Nombre,
                    Municipio = x.Municipio,
                    Estado = x.Estado,
                    Telefono = x.Telefono,
                    Correo = x.Correo,
                    Activo = x.Activo == true
                })
                .ToListAsync(ct);
        }

        // ==========================================
        // DETALLE
        // ==========================================

        public async Task<SucursalDetalleDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default)
        {
            return await context.Sucursals
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId)
                .Select(x => new SucursalDetalleDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,
                    Clave = x.Clave,
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion,
                    Direccion = x.Direccion,
                    Colonia = x.Colonia,
                    Municipio = x.Municipio,
                    Estado = x.Estado,
                    CodigoPostal = x.CodigoPostal,
                    Pais = x.Pais,
                    Telefono = x.Telefono,
                    Correo = x.Correo,
                    Activo = x.Activo == true,
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

        public async Task<ServiceResult<SucursalDetalleDto>> CreateAsync(
            SucursalCreateDto dto,
            Guid empresaId,
            Guid usuarioCreacion,
            CancellationToken ct = default)
        {
            var clave = dto.Clave?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(clave))
            {
                return ServiceResult<SucursalDetalleDto>
                    .Fail("La clave de la sucursal es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return ServiceResult<SucursalDetalleDto>
                    .Fail("El nombre de la sucursal es obligatorio.");
            }

            var existe = await context.Sucursals
                .AnyAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.Clave == clave,
                    ct);

            if (existe)
            {
                return ServiceResult<SucursalDetalleDto>
                    .Fail("Ya existe una sucursal con esa clave en la empresa.");
            }

            var sucursal = new Sucursal
            {
                EmpresaId = empresaId,
                Clave = clave,
                Nombre = dto.Nombre.Trim(),
                Descripcion = Normalizar(dto.Descripcion),
                Direccion = Normalizar(dto.Direccion),
                Colonia = Normalizar(dto.Colonia),
                Municipio = Normalizar(dto.Municipio),
                Estado = Normalizar(dto.Estado),
                CodigoPostal = Normalizar(dto.CodigoPostal),
                Pais = Normalizar(dto.Pais),
                Telefono = Normalizar(dto.Telefono),
                Correo = Normalizar(dto.Correo),

                Activo = true,
                UsuarioCreacion = usuarioCreacion
            };

            context.Sucursals.Add(sucursal);

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (EsClaveDuplicada(ex))
            {
                return ServiceResult<SucursalDetalleDto>
                    .Fail("Ya existe una sucursal con esa clave en la empresa.");
            }

            var detalle = await GetByIdAsync(sucursal.Id, empresaId, ct);

            return ServiceResult<SucursalDetalleDto>.Ok(detalle!);
        }

        // ==========================================
        // ACTUALIZAR
        // ==========================================

        public async Task<ServiceResult<SucursalDetalleDto>> UpdateAsync(
            Guid id,
            SucursalUpdateDto dto,
            Guid empresaId,
            Guid usuarioModificacion,
            CancellationToken ct = default)
        {
            var sucursal = await context.Sucursals
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId,
                    ct);

            if (sucursal == null)
            {
                return ServiceResult<SucursalDetalleDto>
                    .Fail("Sucursal no encontrada.");
            }

            var clave = dto.Clave?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(clave))
            {
                return ServiceResult<SucursalDetalleDto>
                    .Fail("La clave de la sucursal es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return ServiceResult<SucursalDetalleDto>
                    .Fail("El nombre de la sucursal es obligatorio.");
            }

            var existe = await context.Sucursals
                .AnyAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.Clave == clave &&
                    x.Id != id,
                    ct);

            if (existe)
            {
                return ServiceResult<SucursalDetalleDto>
                    .Fail("Ya existe otra sucursal con esa clave en la empresa.");
            }

            sucursal.Clave = clave;
            sucursal.Nombre = dto.Nombre.Trim();
            sucursal.Descripcion = Normalizar(dto.Descripcion);
            sucursal.Direccion = Normalizar(dto.Direccion);
            sucursal.Colonia = Normalizar(dto.Colonia);
            sucursal.Municipio = Normalizar(dto.Municipio);
            sucursal.Estado = Normalizar(dto.Estado);
            sucursal.CodigoPostal = Normalizar(dto.CodigoPostal);
            sucursal.Pais = Normalizar(dto.Pais);
            sucursal.Telefono = Normalizar(dto.Telefono);
            sucursal.Correo = Normalizar(dto.Correo);

            sucursal.FechaActualizacion = DateTime.UtcNow;
            sucursal.UsuarioModificacion = usuarioModificacion;

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (EsClaveDuplicada(ex))
            {
                return ServiceResult<SucursalDetalleDto>
                    .Fail("Ya existe otra sucursal con esa clave en la empresa.");
            }

            var detalle = await GetByIdAsync(id, empresaId, ct);

            return ServiceResult<SucursalDetalleDto>.Ok(detalle!);
        }

        // ==========================================
        // ACTIVAR / DESACTIVAR
        // ==========================================

        public async Task<ServiceResult<bool>> UpdateStatusAsync(
            Guid id,
            Guid empresaId,
            bool activo,
            Guid usuarioModificacion,
            CancellationToken ct = default)
        {
            var sucursal = await context.Sucursals
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId,
                    ct);

            if (sucursal == null)
            {
                return ServiceResult<bool>
                    .Fail("Sucursal no encontrada.");
            }

            if (sucursal.Activo == activo)
            {
                return ServiceResult<bool>.Ok(true);
            }

            sucursal.Activo = activo;
            sucursal.FechaActualizacion = DateTime.UtcNow;
            sucursal.UsuarioModificacion = usuarioModificacion;

            await context.SaveChangesAsync(ct);

            return ServiceResult<bool>.Ok(true);
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

        private static bool EsClaveDuplicada(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sqlEx &&
                (sqlEx.Number == 2601 || sqlEx.Number == 2627);
        }
    }
}
