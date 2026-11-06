using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto;
using Nextag.Api.Models.Dto.Activos;
using Nextag.Api.Services.Activos.Interface;

namespace Nextag.Api.Services.Activos.Implementation;

public class ActivoService : IActivoService
{
    private const int EstadoDisponible = 1;
    private const int EstadoBaja = 7;

    private readonly NextagContext context;
    public ActivoService(NextagContext context) => this.context = context;

    // ================================================
    // LISTADO PAGINADO
    // ================================================
    public async Task<ActivoPaginaDto> GetAllAsync(
        Guid empresaId, ActivoFiltroDto filtros, CancellationToken ct = default)
    {
        var query = context.Activos.AsNoTracking()
            .Where(x => x.EmpresaId == empresaId);

        if (filtros.EstadoActivoId.HasValue)
            query = query.Where(x => x.EstadoActivoId == filtros.EstadoActivoId.Value);
        if (filtros.CategoriaActivoId.HasValue)
            query = query.Where(x => x.CategoriaActivoId == filtros.CategoriaActivoId.Value);
        if (filtros.MarcaId.HasValue)
            query = query.Where(x => x.MarcaId == filtros.MarcaId.Value);
        if (filtros.ModeloActivoId.HasValue)
            query = query.Where(x => x.ModeloActivoId == filtros.ModeloActivoId.Value);
        if (filtros.SucursalId.HasValue)
            query = query.Where(x => x.SucursalId == filtros.SucursalId.Value);
        if (filtros.UbicacionId.HasValue)
            query = query.Where(x => x.UbicacionId == filtros.UbicacionId.Value);
        if (filtros.ProveedorId.HasValue)
            query = query.Where(x => x.ProveedorId == filtros.ProveedorId.Value);

        var buscar = filtros.Buscar?.Trim();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            query = query.Where(x =>
                x.Codigo.Contains(buscar) ||
                x.Nombre.Contains(buscar) ||
                (x.NumeroSerie != null && x.NumeroSerie.Contains(buscar)) ||
                (x.NumeroInventario != null && x.NumeroInventario.Contains(buscar)));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(x => x.Codigo)
            .ThenBy(x => x.Id)
            .Skip((filtros.Pagina - 1) * filtros.TamanoPagina)
            .Take(filtros.TamanoPagina)
            .Select(x => new ActivoListDto
            {
                Id = x.Id,
                EmpresaId = x.EmpresaId,
                Codigo = x.Codigo,
                NumeroInventario = x.NumeroInventario,
                NumeroSerie = x.NumeroSerie,
                Nombre = x.Nombre,
                CategoriaActivoId = x.CategoriaActivoId,
                CategoriaActivoNombre = x.CategoriaActivo.Nombre,
                MarcaId = x.MarcaId,
                MarcaNombre = x.Marca != null ? x.Marca.Nombre : null,
                ModeloActivoId = x.ModeloActivoId,
                ModeloActivoNombre = x.ModeloActivo != null ? x.ModeloActivo.Nombre : null,
                SucursalId = x.SucursalId,
                SucursalNombre = x.Sucursal.Nombre,
                UbicacionId = x.UbicacionId,
                UbicacionNombre = x.Ubicacion != null ? x.Ubicacion.Nombre : null,
                EstadoActivoId = x.EstadoActivoId,
                EstadoActivoNombre = x.EstadoActivo.Nombre,
                ProveedorId = x.ProveedorId,
                // El scaffold nombró ProveedorNavigation al proveedor de adquisición.
                ProveedorNombre = x.ProveedorNavigation != null ? x.ProveedorNavigation.Nombre : null,
                TipoPropiedad = x.TipoPropiedad
            })
            .ToListAsync(ct);

        return new ActivoPaginaDto
        {
            Items = items,
            Total = total,
            Pagina = filtros.Pagina,
            TamanoPagina = filtros.TamanoPagina
        };
    }

    // ================================================
    // DETALLE
    // ================================================
    public Task<ActivoDetalleDto?> GetByIdAsync(
        Guid id, Guid empresaId, CancellationToken ct = default)
    {
        return context.Activos.AsNoTracking()
            .Where(x => x.Id == id && x.EmpresaId == empresaId)
            .Select(x => new ActivoDetalleDto
            {
                Id = x.Id,
                EmpresaId = x.EmpresaId,
                Codigo = x.Codigo,
                NumeroInventario = x.NumeroInventario,
                NumeroSerie = x.NumeroSerie,
                Nombre = x.Nombre,
                Descripcion = x.Descripcion,
                CategoriaActivoId = x.CategoriaActivoId,
                CategoriaActivoNombre = x.CategoriaActivo.Nombre,
                MarcaId = x.MarcaId,
                MarcaNombre = x.Marca != null ? x.Marca.Nombre : null,
                ModeloActivoId = x.ModeloActivoId,
                ModeloActivoNombre = x.ModeloActivo != null ? x.ModeloActivo.Nombre : null,
                SucursalId = x.SucursalId,
                SucursalNombre = x.Sucursal.Nombre,
                UbicacionId = x.UbicacionId,
                UbicacionNombre = x.Ubicacion != null ? x.Ubicacion.Nombre : null,
                EstadoActivoId = x.EstadoActivoId,
                EstadoActivoNombre = x.EstadoActivo.Nombre,
                ProveedorId = x.ProveedorId,
                ProveedorNombre = x.ProveedorNavigation != null ? x.ProveedorNavigation.Nombre : null,
                TipoPropiedad = x.TipoPropiedad,
                PropietarioProveedorId = x.PropietarioProveedorId,
                // El scaffold nombró Proveedor al propietario externo.
                PropietarioProveedorNombre = x.Proveedor != null ? x.Proveedor.Nombre : null,
                PropietarioNombre = x.PropietarioNombre,
                FechaAdquisicion = x.FechaAdquisicion,
                CostoAdquisicion = x.CostoAdquisicion,
                Moneda = x.Moneda,
                FechaAlta = x.FechaAlta,
                FechaBaja = x.FechaBaja,
                Observaciones = x.Observaciones,
                FechaCreacion = x.FechaCreacion,
                UsuarioCreacion = x.UsuarioCreacion,
                FechaActualizacion = x.FechaActualizacion,
                UsuarioModificacion = x.UsuarioModificacion
            })
            .FirstOrDefaultAsync(ct);
    }

    // ================================================
    // CREAR: EstadoActivoId=1 (Disponible), sin etiqueta aún
    // ================================================
    public async Task<ServiceResult<ActivoDetalleDto>> CreateAsync(
        ActivoGuardarDto dto, Guid empresaId, Guid usuarioId,
        CancellationToken ct = default)
    {
        var error = await ValidarDatosAsync(dto, empresaId, null, ct);
        if (error is not null) return ServiceResult<ActivoDetalleDto>.Fail(error);

        var ahora = DateTime.UtcNow;
        var activo = new Activo
        {
            EmpresaId = empresaId,
            EstadoActivoId = EstadoDisponible,
            FechaAlta = ahora,
            FechaCreacion = ahora,
            UsuarioCreacion = usuarioId
        };
        MapearCampos(activo, dto);
        context.Activos.Add(activo);

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (EsDuplicado(ex))
        {
            return ServiceResult<ActivoDetalleDto>.Fail(
                "Ya existe un activo con ese código en la empresa.");
        }
        catch (DbUpdateException ex) when (EsRestriccion(ex))
        {
            return ServiceResult<ActivoDetalleDto>.Fail(
                "No fue posible registrar el activo. Verifica los catálogos y las reglas de la base de datos.");
        }

        var detalle = await GetByIdAsync(activo.Id, empresaId, ct);
        return ServiceResult<ActivoDetalleDto>.Ok(detalle!);
    }

    // ================================================
    // EDITAR datos básicos; el estado no se actualiza por PUT
    // ================================================
    public async Task<ServiceResult<ActivoDetalleDto>> UpdateAsync(
        Guid id, ActivoGuardarDto dto, Guid empresaId, Guid usuarioId,
        CancellationToken ct = default)
    {
        var activo = await context.Activos.FirstOrDefaultAsync(x =>
            x.Id == id && x.EmpresaId == empresaId, ct);

        if (activo is null)
            return ServiceResult<ActivoDetalleDto>.Fail("Activo no encontrado.");
        if (activo.EstadoActivoId == EstadoBaja)
            return ServiceResult<ActivoDetalleDto>.Fail(
                "No se pueden modificar activos dados de baja.");

        // Para activos asignados, reservados, en mantenimiento o en tránsito,
        // el traslado NO puede hacerse desde este formulario.
        if (activo.EstadoActivoId != EstadoDisponible &&
            (activo.SucursalId != dto.SucursalId || activo.UbicacionId != dto.UbicacionId))
        {
            return ServiceResult<ActivoDetalleDto>.Fail(
                "La ubicación de este activo debe modificarse mediante un movimiento.");
        }

        var error = await ValidarDatosAsync(dto, empresaId, id, ct);
        if (error is not null) return ServiceResult<ActivoDetalleDto>.Fail(error);

        MapearCampos(activo, dto);
        activo.FechaActualizacion = DateTime.UtcNow;
        activo.UsuarioModificacion = usuarioId;

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (EsDuplicado(ex))
        {
            return ServiceResult<ActivoDetalleDto>.Fail(
                "Ya existe otro activo con ese código en la empresa.");
        }
        catch (DbUpdateException ex) when (EsRestriccion(ex))
        {
            return ServiceResult<ActivoDetalleDto>.Fail(
                "No fue posible actualizar el activo. Verifica los catálogos y las reglas de la base de datos.");
        }

        var detalle = await GetByIdAsync(id, empresaId, ct);
        return ServiceResult<ActivoDetalleDto>.Ok(detalle!);
    }

    // ================================================
    // VALIDACIONES
    // ================================================
    private async Task<string?> ValidarDatosAsync(
        ActivoGuardarDto dto, Guid empresaId, Guid? excluirId, CancellationToken ct)
    {
        var codigo = dto.Codigo?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(codigo)) return "El código del activo es obligatorio.";
        if (string.IsNullOrWhiteSpace(dto.Nombre)) return "El nombre del activo es obligatorio.";

        var duplicado = await context.Activos.AsNoTracking().AnyAsync(x =>
            x.EmpresaId == empresaId && x.Codigo == codigo &&
            (!excluirId.HasValue || x.Id != excluirId.Value), ct);
        if (duplicado) return "Ya existe un activo con ese código en la empresa.";

        var tipo = dto.TipoPropiedad?.Trim().ToUpperInvariant();
        if (tipo is not ("PROPIO" or "ARRENDADO" or "COMODATO" or "TERCERO"))
            return "Tipo de propiedad inválido (PROPIO, ARRENDADO, COMODATO o TERCERO).";

        var moneda = dto.Moneda?.Trim().ToUpperInvariant();
        if (moneda is null || moneda.Length != 3 ||
            moneda.Any(c => c < 'A' || c > 'Z'))
            return "La moneda debe ser un código de tres letras (por ejemplo, MXN o USD).";

        if (dto.CostoAdquisicion.HasValue &&
            (dto.CostoAdquisicion.Value < 0 ||
             dto.CostoAdquisicion.Value >= 10000000000000000m ||
             decimal.Round(dto.CostoAdquisicion.Value, 2) != dto.CostoAdquisicion.Value))
            return "El costo debe ser no negativo, con máximo dos decimales y dentro del rango permitido.";

        if (dto.PropietarioProveedorId.HasValue && !string.IsNullOrWhiteSpace(dto.PropietarioNombre))
            return "Selecciona un proveedor propietario o escribe un nombre externo, no ambos.";

        if (dto.CategoriaActivoId == Guid.Empty) return "Selecciona una categoría de activo.";
        if (dto.SucursalId == Guid.Empty) return "Selecciona una sucursal.";

        var errorCategoria = await ValidarCategoriaAsync(empresaId, dto.CategoriaActivoId, ct);
        if (errorCategoria is not null) return errorCategoria;

        if (dto.MarcaId.HasValue)
        {
            if (dto.MarcaId.Value == Guid.Empty || !await context.Marcas.AsNoTracking().AnyAsync(x =>
                x.EmpresaId == empresaId && x.Id == dto.MarcaId.Value && x.Activo == true, ct))
                return "La marca no existe en la empresa o está inactiva.";
        }

        if (dto.ModeloActivoId.HasValue)
        {
            if (!dto.MarcaId.HasValue)
                return "Debes seleccionar una marca cuando eliges un modelo.";
            if (dto.ModeloActivoId.Value == Guid.Empty ||
                !await context.ModeloActivos.AsNoTracking().AnyAsync(x =>
                    x.EmpresaId == empresaId && x.Id == dto.ModeloActivoId.Value &&
                    x.MarcaId == dto.MarcaId.Value &&
                    x.CategoriaActivoId == dto.CategoriaActivoId && x.Activo == true, ct))
                return "El modelo no existe, está inactivo o no corresponde a la marca y categoría.";
        }

        var sucursalValida = await context.Sucursals.AsNoTracking().AnyAsync(x =>
            x.EmpresaId == empresaId && x.Id == dto.SucursalId && x.Activo == true, ct);
        if (!sucursalValida) return "La sucursal no existe en la empresa o está inactiva.";

        if (dto.UbicacionId.HasValue)
        {
            var errorUbicacion = await ValidarUbicacionAsync(
                empresaId, dto.SucursalId, dto.UbicacionId.Value, ct);
            if (errorUbicacion is not null) return errorUbicacion;
        }

        if (dto.ProveedorId.HasValue)
        {
            if (dto.ProveedorId.Value == Guid.Empty ||
                !await context.Proveedors.AsNoTracking().AnyAsync(x =>
                    x.Id == dto.ProveedorId.Value && x.EmpresaId == empresaId && x.Activo == true, ct))
                return "El proveedor de adquisición no existe en la empresa o está inactivo.";
        }
        if (dto.PropietarioProveedorId.HasValue)
        {
            if (dto.PropietarioProveedorId.Value == Guid.Empty ||
                !await context.Proveedors.AsNoTracking().AnyAsync(x =>
                    x.Id == dto.PropietarioProveedorId.Value && x.EmpresaId == empresaId && x.Activo == true, ct))
                return "El proveedor propietario no existe en la empresa o está inactivo.";
        }

        return null;
    }

    private async Task<string?> ValidarCategoriaAsync(
        Guid empresaId, Guid categoriaId, CancellationToken ct)
    {
        var visitados = new HashSet<Guid>();
        Guid? actualId = categoriaId;
        while (actualId.HasValue)
        {
            if (!visitados.Add(actualId.Value))
                return "La jerarquía de categorías contiene un ciclo.";
            var categoria = await context.CategoriaActivos.AsNoTracking()
                .Where(x => x.EmpresaId == empresaId && x.Id == actualId.Value)
                .Select(x => new { x.Activo, x.CategoriaPadreId })
                .FirstOrDefaultAsync(ct);
            if (categoria is null)
                return "La categoría no existe en la empresa.";
            if (categoria.Activo != true)
                return "La categoría o alguna categoría superior está inactiva.";
            actualId = categoria.CategoriaPadreId;
        }
        return null;
    }

    private async Task<string?> ValidarUbicacionAsync(
        Guid empresaId, Guid sucursalId, Guid ubicacionId, CancellationToken ct)
    {
        var visitados = new HashSet<Guid>();
        Guid? actualId = ubicacionId;
        while (actualId.HasValue)
        {
            if (!visitados.Add(actualId.Value))
                return "La jerarquía de ubicaciones contiene un ciclo.";
            var ubicacion = await context.Ubicacions.AsNoTracking()
                .Where(x => x.EmpresaId == empresaId && x.SucursalId == sucursalId &&
                    x.Id == actualId.Value)
                .Select(x => new { x.Activo, x.UbicacionPadreId })
                .FirstOrDefaultAsync(ct);
            if (ubicacion is null)
                return "La ubicación no existe o no pertenece a la sucursal seleccionada.";
            if (ubicacion.Activo != true)
                return "La ubicación o alguna ubicación superior está inactiva.";
            actualId = ubicacion.UbicacionPadreId;
        }
        return null;
    }

    private static void MapearCampos(Activo a, ActivoGuardarDto dto)
    {
        a.Codigo = dto.Codigo.Trim().ToUpperInvariant();
        a.NumeroInventario = Normalizar(dto.NumeroInventario);
        a.NumeroSerie = Normalizar(dto.NumeroSerie);
        a.Nombre = dto.Nombre.Trim();
        a.Descripcion = Normalizar(dto.Descripcion);
        a.CategoriaActivoId = dto.CategoriaActivoId;
        a.MarcaId = dto.MarcaId;
        a.ModeloActivoId = dto.ModeloActivoId;
        a.SucursalId = dto.SucursalId;
        a.UbicacionId = dto.UbicacionId;
        a.ProveedorId = dto.ProveedorId;
        a.TipoPropiedad = dto.TipoPropiedad.Trim().ToUpperInvariant();
        a.PropietarioProveedorId = dto.PropietarioProveedorId;
        a.PropietarioNombre = Normalizar(dto.PropietarioNombre);
        a.FechaAdquisicion = dto.FechaAdquisicion?.Date;
        a.CostoAdquisicion = dto.CostoAdquisicion;
        a.Moneda = dto.Moneda.Trim().ToUpperInvariant();
        a.Observaciones = Normalizar(dto.Observaciones);
    }

    private static string? Normalizar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static bool EsDuplicado(DbUpdateException ex) =>
        ex.InnerException is SqlException sql && sql.Number is (2601 or 2627);

    private static bool EsRestriccion(DbUpdateException ex) =>
        ex.InnerException is SqlException sql && sql.Number == 547;
}
