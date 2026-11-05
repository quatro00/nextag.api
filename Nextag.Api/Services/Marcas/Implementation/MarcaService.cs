using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto.Marcas;
using Nextag.Api.Models.Dto;
using Nextag.Api.Services.Marcas.Interface;

namespace Nextag.Api.Services.Marcas.Implementation
{
    public class MarcaService : IMarcaService
    {
        private readonly NextagContext context;

        public MarcaService(NextagContext context)
        {
            this.context = context;
        }

        // ==========================================
        // LISTADO
        // ==========================================

        public async Task<List<MarcaListDto>> GetAllAsync(
            Guid empresaId,
            bool? activo = null,
            CancellationToken ct = default)
        {
            var query = context.Marcas
                .AsNoTracking()
                .Where(x => x.EmpresaId == empresaId);

            if (activo.HasValue)
            {
                query = query.Where(x =>
                    x.Activo == activo.Value);
            }

            return await query
                .OrderBy(x => x.Nombre)
                .Select(x => new MarcaListDto
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

        public async Task<MarcaDetalleDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default)
        {
            return await context.Marcas
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId)
                .Select(x => new MarcaDetalleDto
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

        public async Task<ServiceResult<MarcaDetalleDto>> CreateAsync(
            MarcaCreateDto dto,
            Guid empresaId,
            Guid usuarioCreacion,
            CancellationToken ct = default)
        {
            var clave = dto.Clave?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(clave))
            {
                return ServiceResult<MarcaDetalleDto>
                    .Fail("La clave de la marca es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return ServiceResult<MarcaDetalleDto>
                    .Fail("El nombre de la marca es obligatorio.");
            }

            // VALIDAR CLAVE DUPLICADA
            var existe = await context.Marcas
                .AnyAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.Clave == clave,
                    ct);

            if (existe)
            {
                return ServiceResult<MarcaDetalleDto>
                    .Fail("Ya existe una marca con esa clave en la empresa.");
            }

            // CREAR
            var marca = new Marca
            {
                EmpresaId = empresaId,

                Clave = clave,
                Nombre = dto.Nombre.Trim(),
                Descripcion = Normalizar(dto.Descripcion),

                Activo = true,

                UsuarioCreacion = usuarioCreacion
            };

            context.Marcas.Add(marca);

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (EsClaveDuplicada(ex))
            {
                return ServiceResult<MarcaDetalleDto>
                    .Fail("Ya existe una marca con esa clave en la empresa.");
            }

            var detalle = await GetByIdAsync(
                marca.Id,
                empresaId,
                ct);

            return ServiceResult<MarcaDetalleDto>
                .Ok(detalle!);
        }

        // ==========================================
        // ACTUALIZAR
        // ==========================================

        public async Task<ServiceResult<MarcaDetalleDto>> UpdateAsync(
            Guid id,
            MarcaUpdateDto dto,
            Guid empresaId,
            Guid usuarioModificacion,
            CancellationToken ct = default)
        {
            var marca = await context.Marcas
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId,
                    ct);

            if (marca == null)
            {
                return ServiceResult<MarcaDetalleDto>
                    .Fail("Marca no encontrada.");
            }

            var clave = dto.Clave?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(clave))
            {
                return ServiceResult<MarcaDetalleDto>
                    .Fail("La clave de la marca es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return ServiceResult<MarcaDetalleDto>
                    .Fail("El nombre de la marca es obligatorio.");
            }

            // VALIDAR CLAVE DUPLICADA
            var existe = await context.Marcas
                .AnyAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.Clave == clave &&
                    x.Id != id,
                    ct);

            if (existe)
            {
                return ServiceResult<MarcaDetalleDto>
                    .Fail("Ya existe otra marca con esa clave en la empresa.");
            }

            // ACTUALIZAR
            marca.Clave = clave;
            marca.Nombre = dto.Nombre.Trim();
            marca.Descripcion = Normalizar(dto.Descripcion);

            marca.FechaActualizacion = DateTime.UtcNow;
            marca.UsuarioModificacion = usuarioModificacion;

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (EsClaveDuplicada(ex))
            {
                return ServiceResult<MarcaDetalleDto>
                    .Fail("Ya existe otra marca con esa clave en la empresa.");
            }

            var detalle = await GetByIdAsync(
                id,
                empresaId,
                ct);

            return ServiceResult<MarcaDetalleDto>
                .Ok(detalle!);
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
            var marca = await context.Marcas
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId,
                    ct);

            if (marca == null)
            {
                return ServiceResult<bool>
                    .Fail("Marca no encontrada.");
            }

            if (marca.Activo == activo)
            {
                return ServiceResult<bool>.Ok(true);
            }

            // IMPEDIR DESACTIVACIÓN CON MODELOS ACTIVOS
            if (!activo)
            {
                var tieneModelosActivos = await context.ModeloActivos
                    .AnyAsync(x =>
                        x.EmpresaId == empresaId &&
                        x.MarcaId == id &&
                        x.Activo == true,
                        ct);

                if (tieneModelosActivos)
                {
                    return ServiceResult<bool>
                        .Fail(
                            "No puedes desactivar una marca que tiene modelos activos."
                        );
                }
            }

            // CAMBIAR ESTATUS
            marca.Activo = activo;

            marca.FechaActualizacion = DateTime.UtcNow;
            marca.UsuarioModificacion = usuarioModificacion;

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
                (
                    sqlEx.Number == 2601 ||
                    sqlEx.Number == 2627
                );
        }
    }
}
