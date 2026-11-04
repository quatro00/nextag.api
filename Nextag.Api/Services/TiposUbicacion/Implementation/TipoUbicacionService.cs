using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto.TiposUbicacion;
using Nextag.Api.Models.Dto;
using Nextag.Api.Services.TiposUbicacion.Interface;

namespace Nextag.Api.Services.TiposUbicacion.Implementation
{
    public class TipoUbicacionService : ITipoUbicacionService
    {
        private readonly NextagContext context;

        public TipoUbicacionService(NextagContext context)
        {
            this.context = context;
        }

        // ==========================================
        // LISTADO
        // ==========================================

        public async Task<List<TipoUbicacionListDto>> GetAllAsync(
            Guid empresaId,
            bool? activo = null,
            CancellationToken ct = default)
        {
            var query = context.TipoUbicacions
                .AsNoTracking()
                .Where(x => x.EmpresaId == empresaId);

            if (activo.HasValue)
            {
                query = query.Where(x => x.Activo == activo.Value);
            }

            return await query
                .OrderBy(x => x.Nombre)
                .Select(x => new TipoUbicacionListDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,
                    Clave = x.Clave,
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion,
                    Activo = x.Activo == true
                })
                .ToListAsync(ct);
        }

        // ==========================================
        // DETALLE
        // ==========================================

        public async Task<TipoUbicacionDetalleDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default)
        {
            return await context.TipoUbicacions
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId)
                .Select(x => new TipoUbicacionDetalleDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,
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

        // ==========================================
        // CREAR
        // ==========================================

        public async Task<ServiceResult<TipoUbicacionDetalleDto>> CreateAsync(
            TipoUbicacionCreateDto dto,
            Guid empresaId,
            Guid usuarioCreacion,
            CancellationToken ct = default)
        {
            var clave = dto.Clave?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(clave))
            {
                return ServiceResult<TipoUbicacionDetalleDto>
                    .Fail("La clave del tipo de ubicación es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return ServiceResult<TipoUbicacionDetalleDto>
                    .Fail("El nombre del tipo de ubicación es obligatorio.");
            }

            var existe = await context.TipoUbicacions
                .AnyAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.Clave == clave,
                    ct);

            if (existe)
            {
                return ServiceResult<TipoUbicacionDetalleDto>
                    .Fail("Ya existe un tipo de ubicación con esa clave en la empresa.");
            }

            var tipo = new TipoUbicacion
            {
                EmpresaId = empresaId,

                Clave = clave,
                Nombre = dto.Nombre.Trim(),
                Descripcion = Normalizar(dto.Descripcion),

                Activo = true,

                UsuarioCreacion = usuarioCreacion
            };

            context.TipoUbicacions.Add(tipo);

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (EsClaveDuplicada(ex))
            {
                return ServiceResult<TipoUbicacionDetalleDto>
                    .Fail("Ya existe un tipo de ubicación con esa clave en la empresa.");
            }

            var detalle = await GetByIdAsync(
                tipo.Id,
                empresaId,
                ct);

            return ServiceResult<TipoUbicacionDetalleDto>.Ok(detalle!);
        }

        // ==========================================
        // ACTUALIZAR
        // ==========================================

        public async Task<ServiceResult<TipoUbicacionDetalleDto>> UpdateAsync(
            Guid id,
            TipoUbicacionUpdateDto dto,
            Guid empresaId,
            Guid usuarioModificacion,
            CancellationToken ct = default)
        {
            var tipo = await context.TipoUbicacions
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId,
                    ct);

            if (tipo == null)
            {
                return ServiceResult<TipoUbicacionDetalleDto>
                    .Fail("Tipo de ubicación no encontrado.");
            }

            var clave = dto.Clave?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(clave))
            {
                return ServiceResult<TipoUbicacionDetalleDto>
                    .Fail("La clave del tipo de ubicación es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return ServiceResult<TipoUbicacionDetalleDto>
                    .Fail("El nombre del tipo de ubicación es obligatorio.");
            }

            var existe = await context.TipoUbicacions
                .AnyAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.Clave == clave &&
                    x.Id != id,
                    ct);

            if (existe)
            {
                return ServiceResult<TipoUbicacionDetalleDto>
                    .Fail("Ya existe otro tipo de ubicación con esa clave en la empresa.");
            }

            tipo.Clave = clave;
            tipo.Nombre = dto.Nombre.Trim();
            tipo.Descripcion = Normalizar(dto.Descripcion);

            tipo.FechaActualizacion = DateTime.UtcNow;
            tipo.UsuarioModificacion = usuarioModificacion;

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (EsClaveDuplicada(ex))
            {
                return ServiceResult<TipoUbicacionDetalleDto>
                    .Fail("Ya existe otro tipo de ubicación con esa clave en la empresa.");
            }

            var detalle = await GetByIdAsync(
                id,
                empresaId,
                ct);

            return ServiceResult<TipoUbicacionDetalleDto>.Ok(detalle!);
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
            var tipo = await context.TipoUbicacions
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId,
                    ct);

            if (tipo == null)
            {
                return ServiceResult<bool>
                    .Fail("Tipo de ubicación no encontrado.");
            }

            if (tipo.Activo == activo)
            {
                return ServiceResult<bool>.Ok(true);
            }

            tipo.Activo = activo;
            tipo.FechaActualizacion = DateTime.UtcNow;
            tipo.UsuarioModificacion = usuarioModificacion;

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
