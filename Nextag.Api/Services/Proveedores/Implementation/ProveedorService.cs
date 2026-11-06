using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto;
using Nextag.Api.Models.Dto.Proveedores;
using Nextag.Api.Services.Proveedores.Interface;

namespace Nextag.Api.Services.Proveedores.Implementation;

public class ProveedorService : IProveedorService
{
    private readonly NextagContext context;
    public ProveedorService(NextagContext context) => this.context = context;

    public Task<List<ProveedorListDto>> GetAllAsync(
        Guid empresaId, bool? activo = null, int? tipoProveedorId = null,
        string? buscar = null, CancellationToken ct = default)
    {
        var query = context.Proveedors.AsNoTracking()
            .Where(x => x.EmpresaId == empresaId);

        if (activo.HasValue)
            query = query.Where(x => x.Activo == activo.Value);

        if (tipoProveedorId.HasValue)
            query = query.Where(x => x.ProveedorTipos.Any(t =>
                t.TipoProveedorId == tipoProveedorId.Value));

        buscar = buscar?.Trim();
        if (!string.IsNullOrWhiteSpace(buscar))
            query = query.Where(x => x.Clave.Contains(buscar) ||
                x.Nombre.Contains(buscar) ||
                (x.RazonSocial != null && x.RazonSocial.Contains(buscar)) ||
                (x.Rfc != null && x.Rfc.Contains(buscar)));

        return query.OrderBy(x => x.Nombre).Select(x => new ProveedorListDto
        {
            Id = x.Id,
            EmpresaId = x.EmpresaId,
            Clave = x.Clave,
            Nombre = x.Nombre,
            RazonSocial = x.RazonSocial,
            Rfc = x.Rfc,
            Correo = x.Correo,
            Telefono = x.Telefono,
            Activo = x.Activo == true,
            TipoProveedorIds = x.ProveedorTipos.Select(t => t.TipoProveedorId).ToList(),
            TiposProveedor = x.ProveedorTipos.Select(t => t.TipoProveedor.Nombre)
                .OrderBy(nombre => nombre).ToList()
        }).ToListAsync(ct);
    }

    public Task<ProveedorDetalleDto?> GetByIdAsync(
        Guid id, Guid empresaId, CancellationToken ct = default)
    {
        return context.Proveedors.AsNoTracking()
            .Where(x => x.Id == id && x.EmpresaId == empresaId)
            .Select(x => new ProveedorDetalleDto
            {
                Id = x.Id,
                EmpresaId = x.EmpresaId,
                Clave = x.Clave,
                Nombre = x.Nombre,
                RazonSocial = x.RazonSocial,
                Rfc = x.Rfc,
                Correo = x.Correo,
                Telefono = x.Telefono,
                SitioWeb = x.SitioWeb,
                Direccion = x.Direccion,
                Colonia = x.Colonia,
                Municipio = x.Municipio,
                Estado = x.Estado,
                Pais = x.Pais,
                CodigoPostal = x.CodigoPostal,
                Observaciones = x.Observaciones,
                Activo = x.Activo == true,
                FechaCreacion = x.FechaCreacion,
                UsuarioCreacion = x.UsuarioCreacion,
                FechaActualizacion = x.FechaActualizacion,
                UsuarioModificacion = x.UsuarioModificacion,
                TipoProveedorIds = x.ProveedorTipos.Select(t => t.TipoProveedorId).ToList(),
                TiposProveedor = x.ProveedorTipos.Select(t => t.TipoProveedor.Nombre)
                    .OrderBy(nombre => nombre).ToList()
            }).FirstOrDefaultAsync(ct);
    }

    public async Task<ServiceResult<ProveedorDetalleDto>> CreateAsync(
        ProveedorGuardarDto dto, Guid empresaId, Guid usuarioId,
        CancellationToken ct = default)
    {
        var error = await ValidarDatosAsync(dto, empresaId, null, ct);
        if (error is not null) return ServiceResult<ProveedorDetalleDto>.Fail(error);

        var proveedor = new Proveedor
        {
            EmpresaId = empresaId,
            Activo = true,
            UsuarioCreacion = usuarioId,
            FechaCreacion = DateTime.UtcNow
        };
        MapearCampos(proveedor, dto);

        // Se inserta primero el proveedor para recuperar su GUID NEWSEQUENTIALID.
        // Las asociaciones se guardan en la misma transacción.
        await using var tx = await context.Database.BeginTransactionAsync(ct);
        try
        {
            context.Proveedors.Add(proveedor);
            await context.SaveChangesAsync(ct);

            foreach (var tipoId in dto.TipoProveedorIds.Distinct())
            {
                context.ProveedorTipos.Add(new ProveedorTipo
                {
                    EmpresaId = empresaId,
                    ProveedorId = proveedor.Id,
                    TipoProveedorId = tipoId,
                    UsuarioCreacion = usuarioId,
                    FechaCreacion = DateTime.UtcNow
                });
            }
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateException ex) when (EsDuplicado(ex))
        {
            await tx.RollbackAsync(ct);
            return ServiceResult<ProveedorDetalleDto>.Fail(
                "Ya existe un proveedor con esa clave o una relación duplicada.");
        }

        var detalle = await GetByIdAsync(proveedor.Id, empresaId, ct);
        return ServiceResult<ProveedorDetalleDto>.Ok(detalle!);
    }

    public async Task<ServiceResult<ProveedorDetalleDto>> UpdateAsync(
        Guid id, ProveedorGuardarDto dto, Guid empresaId, Guid usuarioId,
        CancellationToken ct = default)
    {
        var proveedor = await context.Proveedors
            .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId, ct);
        if (proveedor is null)
            return ServiceResult<ProveedorDetalleDto>.Fail("Proveedor no encontrado.");

        var error = await ValidarDatosAsync(dto, empresaId, id, ct);
        if (error is not null) return ServiceResult<ProveedorDetalleDto>.Fail(error);

        MapearCampos(proveedor, dto);
        proveedor.FechaActualizacion = DateTime.UtcNow;
        proveedor.UsuarioModificacion = usuarioId;

        // Diferencial: conservar las asociaciones existentes que continúan seleccionadas.
        var actuales = await context.ProveedorTipos
            .Where(x => x.EmpresaId == empresaId && x.ProveedorId == id)
            .ToListAsync(ct);
        var seleccionados = dto.TipoProveedorIds.Distinct().ToHashSet();

        foreach (var anterior in actuales.Where(x => !seleccionados.Contains(x.TipoProveedorId)))
            context.ProveedorTipos.Remove(anterior);

        var existentes = actuales.Select(x => x.TipoProveedorId).ToHashSet();
        foreach (var tipoId in seleccionados.Where(x => !existentes.Contains(x)))
        {
            context.ProveedorTipos.Add(new ProveedorTipo
            {
                EmpresaId = empresaId,
                ProveedorId = id,
                TipoProveedorId = tipoId,
                UsuarioCreacion = usuarioId,
                FechaCreacion = DateTime.UtcNow
            });
        }

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (EsDuplicado(ex))
        {
            return ServiceResult<ProveedorDetalleDto>.Fail(
                "Ya existe otro proveedor con esa clave o una relación duplicada.");
        }

        var detalle = await GetByIdAsync(id, empresaId, ct);
        return ServiceResult<ProveedorDetalleDto>.Ok(detalle!);
    }

    public async Task<ServiceResult<bool>> UpdateStatusAsync(
        Guid id, Guid empresaId, bool activo, Guid usuarioId,
        CancellationToken ct = default)
    {
        var proveedor = await context.Proveedors
            .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId, ct);
        if (proveedor is null)
            return ServiceResult<bool>.Fail("Proveedor no encontrado.");

        if (proveedor.Activo == activo)
            return ServiceResult<bool>.Ok(true);

        proveedor.Activo = activo;
        proveedor.FechaActualizacion = DateTime.UtcNow;
        proveedor.UsuarioModificacion = usuarioId;
        await context.SaveChangesAsync(ct);
        return ServiceResult<bool>.Ok(true);
    }

    private async Task<string?> ValidarDatosAsync(
        ProveedorGuardarDto dto, Guid empresaId, Guid? excluirId, CancellationToken ct)
    {
        var clave = dto.Clave?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(clave)) return "La clave es obligatoria.";
        if (string.IsNullOrWhiteSpace(dto.Nombre)) return "El nombre es obligatorio.";

        var existe = await context.Proveedors.AnyAsync(x =>
            x.EmpresaId == empresaId && x.Clave == clave &&
            (!excluirId.HasValue || x.Id != excluirId.Value), ct);
        if (existe) return "Ya existe un proveedor con esa clave en la empresa.";

        // No se permiten proveedores sin al menos un tipo de servicio.
        var ids = (dto.TipoProveedorIds ?? new List<int>()).Distinct().ToList();
        if (ids.Count == 0) return "Selecciona al menos un tipo de proveedor.";
        if (ids.Any(x => x <= 0)) return "Los tipos de proveedor seleccionados no son válidos.";

        var cuantos = await context.TipoProveedors.CountAsync(x =>
            ids.Contains(x.Id) && x.Activo == true, ct);
        if (cuantos != ids.Count)
            return "Uno o más tipos de proveedor no existen o están inactivos.";

        return null;
    }

    private static void MapearCampos(Proveedor p, ProveedorGuardarDto dto)
    {
        p.Clave = dto.Clave.Trim().ToUpperInvariant();
        p.Nombre = dto.Nombre.Trim();
        p.RazonSocial = Normalizar(dto.RazonSocial);
        p.Rfc = Normalizar(dto.Rfc)?.ToUpperInvariant();
        p.Correo = Normalizar(dto.Correo);
        p.Telefono = Normalizar(dto.Telefono);
        p.SitioWeb = Normalizar(dto.SitioWeb);
        p.Direccion = Normalizar(dto.Direccion);
        p.Colonia = Normalizar(dto.Colonia);
        p.Municipio = Normalizar(dto.Municipio);
        p.Estado = Normalizar(dto.Estado);
        p.Pais = Normalizar(dto.Pais);
        p.CodigoPostal = Normalizar(dto.CodigoPostal);
        p.Observaciones = Normalizar(dto.Observaciones);
    }

    private static string? Normalizar(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool EsDuplicado(DbUpdateException ex) =>
        ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627);
}
