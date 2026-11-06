using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto;
using Nextag.Api.Models.Dto.Proveedores;
using Nextag.Api.Services.ProveedorContactos.Interface;

namespace Nextag.Api.Services.ProveedorContactos.Implementation;

public class ProveedorContactoService : IProveedorContactoService
{
    private readonly NextagContext context;
    public ProveedorContactoService(NextagContext context) => this.context = context;

    public async Task<List<ProveedorContactoDto>?> GetAllAsync(
        Guid proveedorId, Guid empresaId, bool? activo = null,
        CancellationToken ct = default)
    {
        if (!await ExisteProveedorAsync(proveedorId, empresaId, ct)) return null;

        var query = context.ProveedorContactos.AsNoTracking()
            .Where(x => x.EmpresaId == empresaId && x.ProveedorId == proveedorId);
        if (activo.HasValue) query = query.Where(x => x.Activo == activo.Value);

        return await query.OrderByDescending(x => x.EsPrincipal)
            .ThenBy(x => x.Nombre).Select(x => new ProveedorContactoDto
            {
                Id = x.Id,
                EmpresaId = x.EmpresaId,
                ProveedorId = x.ProveedorId,
                Nombre = x.Nombre,
                Cargo = x.Cargo,
                Correo = x.Correo,
                Telefono = x.Telefono,
                Celular = x.Celular,
                EsPrincipal = x.EsPrincipal,
                Activo = x.Activo == true,
                FechaCreacion = x.FechaCreacion,
                UsuarioCreacion = x.UsuarioCreacion,
                FechaActualizacion = x.FechaActualizacion,
                UsuarioModificacion = x.UsuarioModificacion
            }).ToListAsync(ct);
    }

    public Task<ProveedorContactoDto?> GetByIdAsync(
        Guid proveedorId, Guid contactoId, Guid empresaId,
        CancellationToken ct = default)
    {
        return context.ProveedorContactos.AsNoTracking()
            .Where(x => x.Id == contactoId && x.EmpresaId == empresaId &&
                x.ProveedorId == proveedorId)
            .Select(x => new ProveedorContactoDto
            {
                Id = x.Id,
                EmpresaId = x.EmpresaId,
                ProveedorId = x.ProveedorId,
                Nombre = x.Nombre,
                Cargo = x.Cargo,
                Correo = x.Correo,
                Telefono = x.Telefono,
                Celular = x.Celular,
                EsPrincipal = x.EsPrincipal,
                Activo = x.Activo == true,
                FechaCreacion = x.FechaCreacion,
                UsuarioCreacion = x.UsuarioCreacion,
                FechaActualizacion = x.FechaActualizacion,
                UsuarioModificacion = x.UsuarioModificacion
            }).FirstOrDefaultAsync(ct);
    }

    public async Task<ServiceResult<ProveedorContactoDto>> CreateAsync(
        Guid proveedorId, ProveedorContactoGuardarDto dto, Guid empresaId,
        Guid usuarioId, CancellationToken ct = default)
    {
        var proveedor = await context.Proveedors.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == proveedorId && x.EmpresaId == empresaId, ct);
        if (proveedor is null)
            return ServiceResult<ProveedorContactoDto>.Fail("Proveedor no encontrado.");
        if (proveedor.Activo != true)
            return ServiceResult<ProveedorContactoDto>.Fail("No puedes agregar contactos a un proveedor inactivo.");
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return ServiceResult<ProveedorContactoDto>.Fail("El nombre del contacto es obligatorio.");

        var contacto = new ProveedorContacto
        {
            EmpresaId = empresaId,
            ProveedorId = proveedorId,
            Activo = true,
            EsPrincipal = dto.EsPrincipal,
            UsuarioCreacion = usuarioId,
            FechaCreacion = DateTime.UtcNow
        };
        MapearCampos(contacto, dto);

        // Dos SaveChanges dentro de una sola transacción evitan violar
        // el índice único filtrado mientras se sustituye el principal.
        await using var tx = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, ct);
        try
        {
            if (dto.EsPrincipal)
                await QuitarPrincipalesAsync(proveedorId, empresaId, null, usuarioId, ct);

            context.ProveedorContactos.Add(contacto);
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateException ex) when (EsDuplicado(ex))
        {
            await tx.RollbackAsync(ct);
            return ServiceResult<ProveedorContactoDto>.Fail(
                "No fue posible asignar el contacto principal; inténtalo nuevamente.");
        }

        var detalle = await GetByIdAsync(proveedorId, contacto.Id, empresaId, ct);
        return ServiceResult<ProveedorContactoDto>.Ok(detalle!);
    }

    public async Task<ServiceResult<ProveedorContactoDto>> UpdateAsync(
        Guid proveedorId, Guid contactoId, ProveedorContactoGuardarDto dto,
        Guid empresaId, Guid usuarioId, CancellationToken ct = default)
    {
        var proveedor = await context.Proveedors.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == proveedorId && x.EmpresaId == empresaId, ct);
        if (proveedor is null)
            return ServiceResult<ProveedorContactoDto>.Fail("Proveedor no encontrado.");
        if (proveedor.Activo != true)
            return ServiceResult<ProveedorContactoDto>.Fail("No puedes editar contactos de un proveedor inactivo.");
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return ServiceResult<ProveedorContactoDto>.Fail("El nombre del contacto es obligatorio.");

        var contacto = await context.ProveedorContactos.FirstOrDefaultAsync(x =>
            x.Id == contactoId && x.ProveedorId == proveedorId && x.EmpresaId == empresaId, ct);
        if (contacto is null)
            return ServiceResult<ProveedorContactoDto>.Fail("Contacto no encontrado.");
        if (dto.EsPrincipal && contacto.Activo != true)
            return ServiceResult<ProveedorContactoDto>.Fail(
                "No puedes establecer como principal un contacto inactivo.");

        await using var tx = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, ct);
        try
        {
            if (dto.EsPrincipal)
                await QuitarPrincipalesAsync(proveedorId, empresaId, contactoId, usuarioId, ct);

            MapearCampos(contacto, dto);
            contacto.EsPrincipal = dto.EsPrincipal;
            contacto.FechaActualizacion = DateTime.UtcNow;
            contacto.UsuarioModificacion = usuarioId;
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateException ex) when (EsDuplicado(ex))
        {
            await tx.RollbackAsync(ct);
            return ServiceResult<ProveedorContactoDto>.Fail(
                "No fue posible asignar el contacto principal; inténtalo nuevamente.");
        }

        var detalle = await GetByIdAsync(proveedorId, contactoId, empresaId, ct);
        return ServiceResult<ProveedorContactoDto>.Ok(detalle!);
    }

    public async Task<ServiceResult<bool>> UpdateStatusAsync(
        Guid proveedorId, Guid contactoId, Guid empresaId, bool activo,
        Guid usuarioId, CancellationToken ct = default)
    {
        var proveedor = await context.Proveedors.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == proveedorId && x.EmpresaId == empresaId, ct);
        if (proveedor is null)
            return ServiceResult<bool>.Fail("Proveedor no encontrado.");
        if (activo && proveedor.Activo != true)
            return ServiceResult<bool>.Fail("No puedes activar contactos de un proveedor inactivo.");

        var contacto = await context.ProveedorContactos.FirstOrDefaultAsync(x =>
            x.Id == contactoId && x.ProveedorId == proveedorId && x.EmpresaId == empresaId, ct);
        if (contacto is null)
            return ServiceResult<bool>.Fail("Contacto no encontrado.");

        if (contacto.Activo == activo) return ServiceResult<bool>.Ok(true);

        contacto.Activo = activo;
        // Al desactivar se libera el indicador principal.
        // Reactivar no convierte automáticamente al contacto en principal.
        if (!activo) contacto.EsPrincipal = false;
        contacto.FechaActualizacion = DateTime.UtcNow;
        contacto.UsuarioModificacion = usuarioId;
        await context.SaveChangesAsync(ct);
        return ServiceResult<bool>.Ok(true);
    }

    private Task<bool> ExisteProveedorAsync(Guid proveedorId, Guid empresaId,
        CancellationToken ct) => context.Proveedors.AsNoTracking()
            .AnyAsync(x => x.Id == proveedorId && x.EmpresaId == empresaId, ct);

    private async Task QuitarPrincipalesAsync(
        Guid proveedorId, Guid empresaId, Guid? excluirContactoId,
        Guid usuarioId, CancellationToken ct)
    {
        var anteriores = await context.ProveedorContactos.Where(x =>
            x.ProveedorId == proveedorId && x.EmpresaId == empresaId &&
            x.Activo == true && x.EsPrincipal &&
            (!excluirContactoId.HasValue || x.Id != excluirContactoId.Value))
            .ToListAsync(ct);

        if (anteriores.Count == 0) return;

        foreach (var anterior in anteriores)
        {
            anterior.EsPrincipal = false;
            anterior.FechaActualizacion = DateTime.UtcNow;
            anterior.UsuarioModificacion = usuarioId;
        }
        await context.SaveChangesAsync(ct);
    }

    private static void MapearCampos(
        ProveedorContacto c, ProveedorContactoGuardarDto dto)
    {
        c.Nombre = dto.Nombre.Trim();
        c.Cargo = Normalizar(dto.Cargo);
        c.Correo = Normalizar(dto.Correo);
        c.Telefono = Normalizar(dto.Telefono);
        c.Celular = Normalizar(dto.Celular);
    }

    private static string? Normalizar(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool EsDuplicado(DbUpdateException ex) =>
        ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627);
}
