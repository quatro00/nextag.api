using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto.CategoriasActivo;
using Nextag.Api.Models.Dto;
using Nextag.Api.Services.CategoriasActivo.Interface;

namespace Nextag.Api.Services.CategoriasActivo.Implementation
{
    public class CategoriaActivoService : ICategoriaActivoService
    {
        private readonly NextagContext context;

        public CategoriaActivoService(NextagContext context)
        {
            this.context = context;
        }

        // ==========================================
        // LISTADO
        // ==========================================

        public async Task<List<CategoriaActivoListDto>> GetAllAsync(
            Guid empresaId,
            bool? activo = null,
            CancellationToken ct = default)
        {
            var query = context.CategoriaActivos
                .AsNoTracking()
                .Where(x => x.EmpresaId == empresaId);

            if (activo.HasValue)
            {
                query = query.Where(x =>
                    x.Activo == activo.Value);
            }

            return await query
                .OrderBy(x => x.Nombre)
                .Select(x => new CategoriaActivoListDto
                {
                    Id = x.Id,

                    EmpresaId = x.EmpresaId,

                    CategoriaPadreId = x.CategoriaPadreId,

                    CategoriaPadreNombre =
                        x.CategoriaActivoNavigation != null
                            ? x.CategoriaActivoNavigation.Nombre
                            : null,

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

        public async Task<CategoriaActivoDetalleDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default)
        {
            return await context.CategoriaActivos
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId)
                .Select(x => new CategoriaActivoDetalleDto
                {
                    Id = x.Id,

                    EmpresaId = x.EmpresaId,

                    CategoriaPadreId = x.CategoriaPadreId,

                    CategoriaPadreNombre =
                        x.CategoriaActivoNavigation != null
                            ? x.CategoriaActivoNavigation.Nombre
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

        // ==========================================
        // CREAR
        // ==========================================

        public async Task<ServiceResult<CategoriaActivoDetalleDto>> CreateAsync(
            CategoriaActivoCreateDto dto,
            Guid empresaId,
            Guid usuarioCreacion,
            CancellationToken ct = default)
        {
            var clave = dto.Clave?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(clave))
            {
                return ServiceResult<CategoriaActivoDetalleDto>
                    .Fail("La clave de la categoría es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return ServiceResult<CategoriaActivoDetalleDto>
                    .Fail("El nombre de la categoría es obligatorio.");
            }

            // ==================================
            // VALIDAR CATEGORIA PADRE
            // ==================================

            var errorJerarquia = await ValidarJerarquiaAsync(
                empresaId,
                dto.CategoriaPadreId,
                null,
                ct);

            if (errorJerarquia != null)
            {
                return ServiceResult<CategoriaActivoDetalleDto>
                    .Fail(errorJerarquia);
            }

            // ==================================
            // VALIDAR CLAVE UNICA POR EMPRESA
            // ==================================

            var existe = await context.CategoriaActivos
                .AnyAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.Clave == clave,
                    ct);

            if (existe)
            {
                return ServiceResult<CategoriaActivoDetalleDto>
                    .Fail(
                        "Ya existe una categoría con esa clave en la empresa."
                    );
            }

            // ==================================
            // CREAR REGISTRO
            // ==================================

            var categoria = new CategoriaActivo
            {
                EmpresaId = empresaId,

                CategoriaPadreId = dto.CategoriaPadreId,

                Clave = clave,
                Nombre = dto.Nombre.Trim(),

                Descripcion = Normalizar(dto.Descripcion),

                Activo = true,

                UsuarioCreacion = usuarioCreacion
            };

            context.CategoriaActivos.Add(categoria);

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (EsClaveDuplicada(ex))
            {
                return ServiceResult<CategoriaActivoDetalleDto>
                    .Fail(
                        "Ya existe una categoría con esa clave en la empresa."
                    );
            }

            var detalle = await GetByIdAsync(
                categoria.Id,
                empresaId,
                ct);

            return ServiceResult<CategoriaActivoDetalleDto>
                .Ok(detalle!);
        }

        // ==========================================
        // ACTUALIZAR
        // ==========================================

        public async Task<ServiceResult<CategoriaActivoDetalleDto>> UpdateAsync(
            Guid id,
            CategoriaActivoUpdateDto dto,
            Guid empresaId,
            Guid usuarioModificacion,
            CancellationToken ct = default)
        {
            var categoria = await context.CategoriaActivos
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId,
                    ct);

            if (categoria == null)
            {
                return ServiceResult<CategoriaActivoDetalleDto>
                    .Fail("Categoría no encontrada.");
            }

            var clave = dto.Clave?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(clave))
            {
                return ServiceResult<CategoriaActivoDetalleDto>
                    .Fail("La clave de la categoría es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return ServiceResult<CategoriaActivoDetalleDto>
                    .Fail("El nombre de la categoría es obligatorio.");
            }

            // ==================================
            // VALIDAR JERARQUIA
            // ==================================

            var errorJerarquia = await ValidarJerarquiaAsync(
                empresaId,
                dto.CategoriaPadreId,
                id,
                ct);

            if (errorJerarquia != null)
            {
                return ServiceResult<CategoriaActivoDetalleDto>
                    .Fail(errorJerarquia);
            }

            // ==================================
            // VALIDAR CLAVE DUPLICADA
            // ==================================

            var existe = await context.CategoriaActivos
                .AnyAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.Clave == clave &&
                    x.Id != id,
                    ct);

            if (existe)
            {
                return ServiceResult<CategoriaActivoDetalleDto>
                    .Fail(
                        "Ya existe otra categoría con esa clave en la empresa."
                    );
            }

            // ==================================
            // ACTUALIZAR DATOS
            // ==================================

            categoria.Clave = clave;
            categoria.Nombre = dto.Nombre.Trim();

            categoria.Descripcion = Normalizar(dto.Descripcion);

            categoria.CategoriaPadreId = dto.CategoriaPadreId;

            categoria.FechaActualizacion = DateTime.UtcNow;
            categoria.UsuarioModificacion = usuarioModificacion;

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (EsClaveDuplicada(ex))
            {
                return ServiceResult<CategoriaActivoDetalleDto>
                    .Fail(
                        "Ya existe otra categoría con esa clave en la empresa."
                    );
            }

            var detalle = await GetByIdAsync(
                id,
                empresaId,
                ct);

            return ServiceResult<CategoriaActivoDetalleDto>
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
            var categoria = await context.CategoriaActivos
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId,
                    ct);

            if (categoria == null)
            {
                return ServiceResult<bool>
                    .Fail("Categoría no encontrada.");
            }

            // Sin cambios
            if (categoria.Activo == activo)
            {
                return ServiceResult<bool>.Ok(true);
            }

            // ==================================
            // DESACTIVAR
            // ==================================

            if (!activo)
            {
                // ==================================
                // VALIDAR SUBCATEGORIAS ACTIVAS
                // ==================================

                var tieneDescendientesActivos =
                    await TieneDescendientesActivosAsync(
                        empresaId,
                        id,
                        ct);

                if (tieneDescendientesActivos)
                {
                    return ServiceResult<bool>
                        .Fail(
                            "No puedes desactivar una categoría que tiene subcategorías activas."
                        );
                }

                // ==================================
                // VALIDAR MODELOS ACTIVOS
                // ==================================

                var tieneModelosActivos =
                    await context.ModeloActivos
                        .AnyAsync(x =>
                            x.EmpresaId == empresaId &&
                            x.CategoriaActivoId == id &&
                            x.Activo == true,
                            ct);

                if (tieneModelosActivos)
                {
                    return ServiceResult<bool>
                        .Fail(
                            "No puedes desactivar una categoría que tiene modelos de activos activos."
                        );
                }
            }

            // ==================================
            // REACTIVAR
            // ==================================

            else
            {
                var errorJerarquia = await ValidarJerarquiaAsync(
                    empresaId,
                    categoria.CategoriaPadreId,
                    id,
                    ct);

                if (errorJerarquia != null)
                {
                    return ServiceResult<bool>
                        .Fail(errorJerarquia);
                }
            }

            // ==================================
            // GUARDAR ESTATUS
            // ==================================

            categoria.Activo = activo;

            categoria.FechaActualizacion = DateTime.UtcNow;
            categoria.UsuarioModificacion = usuarioModificacion;

            await context.SaveChangesAsync(ct);

            return ServiceResult<bool>.Ok(true);
        }

        // ==========================================
        // VALIDAR JERARQUIA
        // ==========================================

        private async Task<string?> ValidarJerarquiaAsync(
            Guid empresaId,
            Guid? categoriaPadreId,
            Guid? categoriaActualId,
            CancellationToken ct)
        {
            if (!categoriaPadreId.HasValue)
            {
                return null;
            }

            if (categoriaPadreId.Value == Guid.Empty)
            {
                return "La categoría padre no es válida.";
            }

            var visitados = new HashSet<Guid>();

            // Agregamos la categoría actual para detectar
            // si algún padre termina apuntando hacia ella.
            if (categoriaActualId.HasValue)
            {
                visitados.Add(categoriaActualId.Value);
            }

            Guid? actualId = categoriaPadreId;

            while (actualId.HasValue)
            {
                // Si ya visitamos esta categoría,
                // existe una relación circular.
                if (!visitados.Add(actualId.Value))
                {
                    return "La categoría padre genera una relación circular.";
                }

                var padre = await context.CategoriaActivos
                    .AsNoTracking()
                    .Where(x =>
                        x.Id == actualId.Value &&
                        x.EmpresaId == empresaId)
                    .Select(x => new
                    {
                        x.CategoriaPadreId,
                        x.Activo
                    })
                    .FirstOrDefaultAsync(ct);

                if (padre == null)
                {
                    return
                        "La categoría padre no existe o pertenece a otra empresa.";
                }

                if (padre.Activo != true)
                {
                    return
                        "La categoría padre o alguno de sus niveles superiores está inactivo.";
                }

                actualId = padre.CategoriaPadreId;
            }

            return null;
        }

        // ==========================================
        // VALIDAR DESCENDIENTES ACTIVOS
        // ==========================================

        private async Task<bool> TieneDescendientesActivosAsync(
            Guid empresaId,
            Guid categoriaId,
            CancellationToken ct)
        {
            // Consultamos solamente la información necesaria
            // de las categorías de esta empresa.

            var categorias = await context.CategoriaActivos
                .AsNoTracking()
                .Where(x => x.EmpresaId == empresaId)
                .Select(x => new
                {
                    x.Id,
                    x.CategoriaPadreId,
                    x.Activo
                })
                .ToListAsync(ct);

            var categoriasPorPadre = categorias
                .ToLookup(x => x.CategoriaPadreId);

            var pendientes = new Stack<Guid>();
            var visitados = new HashSet<Guid>();

            pendientes.Push(categoriaId);
            visitados.Add(categoriaId);

            while (pendientes.Count > 0)
            {
                var padreId = pendientes.Pop();

                foreach (var hijo in categoriasPorPadre[padreId])
                {
                    if (!visitados.Add(hijo.Id))
                    {
                        continue;
                    }

                    if (hijo.Activo == true)
                    {
                        return true;
                    }

                    // Revisar también los descendientes de
                    // categorías inactivas para detectar
                    // posibles inconsistencias anteriores.
                    pendientes.Push(hijo.Id);
                }
            }

            return false;
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

        private static bool EsClaveDuplicada(
            DbUpdateException ex)
        {
            return ex.InnerException is SqlException sqlEx &&
                (
                    sqlEx.Number == 2601 ||
                    sqlEx.Number == 2627
                );
        }
    }
}
