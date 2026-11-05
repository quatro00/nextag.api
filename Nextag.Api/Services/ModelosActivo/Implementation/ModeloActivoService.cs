using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto.ModelosActivo;
using Nextag.Api.Models.Dto;
using Nextag.Api.Services.ModelosActivo.Interface;

namespace Nextag.Api.Services.ModelosActivo.Implementation
{
    public class ModeloActivoService : IModeloActivoService
    {
        private readonly NextagContext context;

        public ModeloActivoService(NextagContext context)
        {
            this.context = context;
        }

        // ==========================================
        // LISTADO
        // ==========================================

        public async Task<List<ModeloActivoListDto>> GetAllAsync(
            Guid empresaId,
            Guid? marcaId = null,
            Guid? categoriaActivoId = null,
            bool? activo = null,
            CancellationToken ct = default)
        {
            var query = context.ModeloActivos
                .AsNoTracking()
                .Where(x => x.EmpresaId == empresaId);

            if (marcaId.HasValue)
            {
                query = query.Where(x =>
                    x.MarcaId == marcaId.Value);
            }

            if (categoriaActivoId.HasValue)
            {
                query = query.Where(x =>
                    x.CategoriaActivoId == categoriaActivoId.Value);
            }

            if (activo.HasValue)
            {
                query = query.Where(x =>
                    x.Activo == activo.Value);
            }

            return await query
                .OrderBy(x => x.Marca.Nombre)
                .ThenBy(x => x.Nombre)
                .Select(x => new ModeloActivoListDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,

                    MarcaId = x.MarcaId,
                    MarcaNombre = x.Marca.Nombre,

                    CategoriaActivoId = x.CategoriaActivoId,
                    CategoriaActivoNombre = x.CategoriaActivo.Nombre,

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

        public async Task<ModeloActivoDetalleDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default)
        {
            return await context.ModeloActivos
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId)
                .Select(x => new ModeloActivoDetalleDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,

                    MarcaId = x.MarcaId,
                    MarcaNombre = x.Marca.Nombre,

                    CategoriaActivoId = x.CategoriaActivoId,
                    CategoriaActivoNombre = x.CategoriaActivo.Nombre,

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

        public async Task<ServiceResult<ModeloActivoDetalleDto>> CreateAsync(
            ModeloActivoCreateDto dto,
            Guid empresaId,
            Guid usuarioCreacion,
            CancellationToken ct = default)
        {
            var clave = dto.Clave?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(clave))
            {
                return ServiceResult<ModeloActivoDetalleDto>
                    .Fail("La clave del modelo es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return ServiceResult<ModeloActivoDetalleDto>
                    .Fail("El nombre del modelo es obligatorio.");
            }

            // ==================================
            // VALIDAR MARCA Y CATEGORÍA
            // ==================================

            var errorRelaciones = await ValidarRelacionesAsync(
                empresaId,
                dto.MarcaId,
                dto.CategoriaActivoId,
                ct);

            if (errorRelaciones != null)
            {
                return ServiceResult<ModeloActivoDetalleDto>
                    .Fail(errorRelaciones);
            }

            // ==================================
            // VALIDAR CLAVE ÚNICA POR MARCA
            // ==================================

            var existe = await context.ModeloActivos
                .AnyAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.MarcaId == dto.MarcaId &&
                    x.Clave == clave,
                    ct);

            if (existe)
            {
                return ServiceResult<ModeloActivoDetalleDto>
                    .Fail(
                        "Ya existe un modelo con esa clave para la marca seleccionada."
                    );
            }

            // ==================================
            // CREAR
            // ==================================

            var modelo = new ModeloActivo
            {
                EmpresaId = empresaId,

                MarcaId = dto.MarcaId,
                CategoriaActivoId = dto.CategoriaActivoId,

                Clave = clave,
                Nombre = dto.Nombre.Trim(),
                Descripcion = Normalizar(dto.Descripcion),

                Activo = true,

                UsuarioCreacion = usuarioCreacion
            };

            context.ModeloActivos.Add(modelo);

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (EsClaveDuplicada(ex))
            {
                return ServiceResult<ModeloActivoDetalleDto>
                    .Fail(
                        "Ya existe un modelo con esa clave para la marca seleccionada."
                    );
            }

            var detalle = await GetByIdAsync(
                modelo.Id,
                empresaId,
                ct);

            return ServiceResult<ModeloActivoDetalleDto>
                .Ok(detalle!);
        }

        // ==========================================
        // ACTUALIZAR
        // ==========================================

        public async Task<ServiceResult<ModeloActivoDetalleDto>> UpdateAsync(
            Guid id,
            ModeloActivoUpdateDto dto,
            Guid empresaId,
            Guid usuarioModificacion,
            CancellationToken ct = default)
        {
            var modelo = await context.ModeloActivos
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId,
                    ct);

            if (modelo == null)
            {
                return ServiceResult<ModeloActivoDetalleDto>
                    .Fail("Modelo de activo no encontrado.");
            }

            var clave = dto.Clave?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(clave))
            {
                return ServiceResult<ModeloActivoDetalleDto>
                    .Fail("La clave del modelo es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return ServiceResult<ModeloActivoDetalleDto>
                    .Fail("El nombre del modelo es obligatorio.");
            }

            // ==================================
            // VALIDAR MARCA Y CATEGORÍA
            // ==================================

            var errorRelaciones = await ValidarRelacionesAsync(
                empresaId,
                dto.MarcaId,
                dto.CategoriaActivoId,
                ct);

            if (errorRelaciones != null)
            {
                return ServiceResult<ModeloActivoDetalleDto>
                    .Fail(errorRelaciones);
            }

            // ==================================
            // VALIDAR CLAVE DUPLICADA
            // ==================================

            var existe = await context.ModeloActivos
                .AnyAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.MarcaId == dto.MarcaId &&
                    x.Clave == clave &&
                    x.Id != id,
                    ct);

            if (existe)
            {
                return ServiceResult<ModeloActivoDetalleDto>
                    .Fail(
                        "Ya existe otro modelo con esa clave para la marca seleccionada."
                    );
            }

            // ==================================
            // ACTUALIZAR
            // ==================================

            modelo.MarcaId = dto.MarcaId;
            modelo.CategoriaActivoId = dto.CategoriaActivoId;

            modelo.Clave = clave;
            modelo.Nombre = dto.Nombre.Trim();
            modelo.Descripcion = Normalizar(dto.Descripcion);

            modelo.FechaActualizacion = DateTime.UtcNow;
            modelo.UsuarioModificacion = usuarioModificacion;

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (EsClaveDuplicada(ex))
            {
                return ServiceResult<ModeloActivoDetalleDto>
                    .Fail(
                        "Ya existe otro modelo con esa clave para la marca seleccionada."
                    );
            }

            var detalle = await GetByIdAsync(
                id,
                empresaId,
                ct);

            return ServiceResult<ModeloActivoDetalleDto>
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
            var modelo = await context.ModeloActivos
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId,
                    ct);

            if (modelo == null)
            {
                return ServiceResult<bool>
                    .Fail("Modelo de activo no encontrado.");
            }

            if (modelo.Activo == activo)
            {
                return ServiceResult<bool>.Ok(true);
            }

            // ==================================
            // REACTIVAR
            // ==================================

            if (activo)
            {
                var errorRelaciones = await ValidarRelacionesAsync(
                    empresaId,
                    modelo.MarcaId,
                    modelo.CategoriaActivoId,
                    ct);

                if (errorRelaciones != null)
                {
                    return ServiceResult<bool>
                        .Fail(errorRelaciones);
                }
            }

            // ==================================
            // CAMBIAR ESTATUS
            // ==================================

            modelo.Activo = activo;

            modelo.FechaActualizacion = DateTime.UtcNow;
            modelo.UsuarioModificacion = usuarioModificacion;

            await context.SaveChangesAsync(ct);

            return ServiceResult<bool>.Ok(true);
        }

        // ==========================================
        // VALIDAR RELACIONES
        // ==========================================

        private async Task<string?> ValidarRelacionesAsync(
            Guid empresaId,
            Guid marcaId,
            Guid categoriaActivoId,
            CancellationToken ct)
        {
            // ==================================
            // VALIDAR MARCA
            // ==================================

            if (marcaId == Guid.Empty)
            {
                return "Debes seleccionar una marca.";
            }

            var marca = await context.Marcas
                .AsNoTracking()
                .Where(x =>
                    x.Id == marcaId &&
                    x.EmpresaId == empresaId)
                .Select(x => new
                {
                    x.Activo
                })
                .FirstOrDefaultAsync(ct);

            if (marca == null)
            {
                return "La marca seleccionada no existe o pertenece a otra empresa.";
            }

            if (marca.Activo != true)
            {
                return "La marca seleccionada está inactiva.";
            }

            // ==================================
            // VALIDAR CATEGORÍA
            // ==================================

            if (categoriaActivoId == Guid.Empty)
            {
                return "Debes seleccionar una categoría de activo.";
            }

            var visitados = new HashSet<Guid>();

            Guid? categoriaActualId = categoriaActivoId;

            // Recorrer la categoría seleccionada
            // y todos sus padres.
            while (categoriaActualId.HasValue)
            {
                if (!visitados.Add(categoriaActualId.Value))
                {
                    return "La categoría seleccionada tiene una relación circular.";
                }

                var categoria = await context.CategoriaActivos
                    .AsNoTracking()
                    .Where(x =>
                        x.Id == categoriaActualId.Value &&
                        x.EmpresaId == empresaId)
                    .Select(x => new
                    {
                        x.CategoriaPadreId,
                        x.Activo
                    })
                    .FirstOrDefaultAsync(ct);

                if (categoria == null)
                {
                    return "La categoría seleccionada no existe o pertenece a otra empresa.";
                }

                if (categoria.Activo != true)
                {
                    return "La categoría seleccionada o alguno de sus niveles superiores está inactivo.";
                }

                categoriaActualId = categoria.CategoriaPadreId;
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
