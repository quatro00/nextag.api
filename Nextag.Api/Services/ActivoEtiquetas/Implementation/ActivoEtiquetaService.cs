using System.Data;
using System.Linq.Expressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto;
using Nextag.Api.Models.Dto.Etiquetas;
using Nextag.Api.Services.ActivoEtiquetas.Interface;

namespace Nextag.Api.Services.ActivoEtiquetas.Implementation;

public class ActivoEtiquetaService : IActivoEtiquetaService
{
    private const int EstadoBaja = 7;
    private readonly NextagContext context;
    private readonly IConfiguration configuration;

    public ActivoEtiquetaService(NextagContext context, IConfiguration configuration)
    {
        this.context = context;
        this.configuration = configuration;
    }

    private static readonly Expression<Func<ActivoEtiquetum, ActivoEtiquetaHistorialDto>> Proyeccion = x =>
        new ActivoEtiquetaHistorialDto
        {
            Id = x.Id,
            ActivoId = x.ActivoId,
            EtiquetaId = x.EtiquetaId,
            EtiquetaCodigo = x.E.Codigo,
            TokenPublico = x.E.TokenPublico,
            TieneNfc = x.E.TieneNfc == true,
            TieneQr = x.E.TieneQr == true,
            EsAntimetal = x.E.EsAntimetal,
            NfcBloqueado = x.E.NfcBloqueado,
            Anulada = x.E.Anulada,
            Vigente = x.FechaDesvinculacion == null && !x.E.Anulada,
            FechaVinculacion = x.FechaVinculacion,
            UsuarioVinculacion = x.UsuarioVinculacion,
            FechaDesvinculacion = x.FechaDesvinculacion,
            UsuarioDesvinculacion = x.UsuarioDesvinculacion,
            MotivoDesvinculacion = x.MotivoDesvinculacion
        };

    public async Task<List<ActivoEtiquetaHistorialDto>?> GetHistorialAsync(
        Guid activoId, Guid empresaId, CancellationToken ct = default)
    {
        if (!await ExisteActivoAsync(activoId, empresaId, ct)) return null;
        var historial = await context.ActivoEtiqueta.AsNoTracking()
            .Where(x => x.EmpresaId == empresaId && x.ActivoId == activoId)
            .OrderByDescending(x => x.FechaVinculacion)
            .ThenByDescending(x => x.Id)
            .Select(Proyeccion).ToListAsync(ct);
        CompletarUrl(historial);
        return historial;
    }

    public async Task<ActivoEtiquetaHistorialDto?> GetVigenteAsync(
        Guid activoId, Guid empresaId, CancellationToken ct = default)
    {
        var vigente = await context.ActivoEtiqueta.AsNoTracking()
            .Where(x => x.EmpresaId == empresaId && x.ActivoId == activoId &&
                x.FechaDesvinculacion == null && !x.E.Anulada)
            .Select(Proyeccion).FirstOrDefaultAsync(ct);
        if (vigente is not null) CompletarUrl(new[] { vigente });
        return vigente;
    }

    public async Task<ServiceResult<ActivoEtiquetaHistorialDto>> VincularAsync(
        Guid activoId, EtiquetaVincularDto dto, Guid empresaId, Guid usuarioId,
        CancellationToken ct = default)
    {
        if (dto.EtiquetaId == Guid.Empty)
            return ServiceResult<ActivoEtiquetaHistorialDto>.Fail("Selecciona una etiqueta.");

        await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            var errorActivo = await ValidarActivoAsignableAsync(activoId, empresaId, ct);
            if (errorActivo is not null)
                return ServiceResult<ActivoEtiquetaHistorialDto>.Fail(errorActivo);

            var yaTiene = await context.ActivoEtiqueta.AsNoTracking().AnyAsync(x =>
                x.EmpresaId == empresaId && x.ActivoId == activoId &&
                x.FechaDesvinculacion == null, ct);
            if (yaTiene)
                return ServiceResult<ActivoEtiquetaHistorialDto>.Fail(
                    "El activo ya tiene una etiqueta vigente. Utiliza la operación reemplazar.");

            var errorEtiqueta = await ValidarEtiquetaDisponibleAsync(dto.EtiquetaId, empresaId, ct);
            if (errorEtiqueta is not null)
                return ServiceResult<ActivoEtiquetaHistorialDto>.Fail(errorEtiqueta);

            context.ActivoEtiqueta.Add(new ActivoEtiquetum
            {
                EmpresaId = empresaId,
                ActivoId = activoId,
                EtiquetaId = dto.EtiquetaId,
                FechaVinculacion = DateTime.UtcNow,
                UsuarioVinculacion = usuarioId
            });
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (Exception ex) when (EsConflictoSql(ex))
        {
            await tx.RollbackAsync(ct);
            return ServiceResult<ActivoEtiquetaHistorialDto>.Fail(
                "La etiqueta fue vinculada o cambió su estado durante la operación. Vuelve a consultar.");
        }

        var vigente = await GetVigenteAsync(activoId, empresaId, ct);
        return ServiceResult<ActivoEtiquetaHistorialDto>.Ok(vigente!);
    }

    public async Task<ServiceResult<ActivoEtiquetaHistorialDto>> ReemplazarAsync(
        Guid activoId, EtiquetaReemplazarDto dto, Guid empresaId, Guid usuarioId,
        CancellationToken ct = default)
    {
        var motivo = dto.Motivo?.Trim();
        if (string.IsNullOrWhiteSpace(motivo))
            return ServiceResult<ActivoEtiquetaHistorialDto>.Fail("Indica el motivo del reemplazo.");
        if (dto.EtiquetaId == Guid.Empty)
            return ServiceResult<ActivoEtiquetaHistorialDto>.Fail("Selecciona la etiqueta nueva.");

        await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            var errorActivo = await ValidarActivoAsignableAsync(activoId, empresaId, ct);
            if (errorActivo is not null)
                return ServiceResult<ActivoEtiquetaHistorialDto>.Fail(errorActivo);

            var actual = await context.ActivoEtiqueta.FirstOrDefaultAsync(x =>
                x.EmpresaId == empresaId && x.ActivoId == activoId &&
                x.FechaDesvinculacion == null, ct);
            if (actual is null)
                return ServiceResult<ActivoEtiquetaHistorialDto>.Fail(
                    "El activo no tiene etiqueta vigente. Utiliza vincular.");
            if (actual.EtiquetaId == dto.EtiquetaId)
                return ServiceResult<ActivoEtiquetaHistorialDto>.Fail(
                    "La etiqueta nueva debe ser diferente a la actual.");

            var errorEtiqueta = await ValidarEtiquetaDisponibleAsync(dto.EtiquetaId, empresaId, ct);
            if (errorEtiqueta is not null)
                return ServiceResult<ActivoEtiquetaHistorialDto>.Fail(errorEtiqueta);

            var anterior = await context.Etiqueta.FirstAsync(x =>
                x.EmpresaId == empresaId && x.Id == actual.EtiquetaId, ct);
            var ahora = DateTime.UtcNow;

            actual.FechaDesvinculacion = ahora;
            actual.UsuarioDesvinculacion = usuarioId;
            actual.MotivoDesvinculacion = motivo;
            AnularAnterior(anterior, motivo, ahora, usuarioId);

            // Paso 1: liberar índice único filtrado de etiqueta vigente.
            await context.SaveChangesAsync(ct);
            // Paso 2: registrar nueva relación; todo ocurre en la misma transacción.
            context.ActivoEtiqueta.Add(new ActivoEtiquetum
            {
                EmpresaId = empresaId,
                ActivoId = activoId,
                EtiquetaId = dto.EtiquetaId,
                FechaVinculacion = DateTime.UtcNow,
                UsuarioVinculacion = usuarioId
            });
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (Exception ex) when (EsConflictoSql(ex))
        {
            await tx.RollbackAsync(ct);
            return ServiceResult<ActivoEtiquetaHistorialDto>.Fail(
                "Conflicto al reemplazar la etiqueta. No se guardó el reemplazo; vuelve a consultar.");
        }

        var vigente = await GetVigenteAsync(activoId, empresaId, ct);
        return ServiceResult<ActivoEtiquetaHistorialDto>.Ok(vigente!);
    }

    public async Task<ServiceResult<bool>> DesvincularAsync(
        Guid activoId, EtiquetaDesvincularDto dto, Guid empresaId, Guid usuarioId,
        CancellationToken ct = default)
    {
        var motivo = dto.Motivo?.Trim();
        if (string.IsNullOrWhiteSpace(motivo))
            return ServiceResult<bool>.Fail("Indica el motivo de desvinculación.");

        await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            if (!await ExisteActivoAsync(activoId, empresaId, ct))
                return ServiceResult<bool>.Fail("Activo no encontrado.");
            var actual = await context.ActivoEtiqueta.FirstOrDefaultAsync(x =>
                x.EmpresaId == empresaId && x.ActivoId == activoId &&
                x.FechaDesvinculacion == null, ct);
            if (actual is null)
                return ServiceResult<bool>.Fail("El activo no tiene etiqueta vigente.");

            var anterior = await context.Etiqueta.FirstAsync(x =>
                x.EmpresaId == empresaId && x.Id == actual.EtiquetaId, ct);
            var ahora = DateTime.UtcNow;
            actual.FechaDesvinculacion = ahora;
            actual.UsuarioDesvinculacion = usuarioId;
            actual.MotivoDesvinculacion = motivo;
            // Por política, una etiqueta que estuvo asociada NUNCA se reutiliza.
            AnularAnterior(anterior, motivo, ahora, usuarioId);
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception ex) when (EsConflictoSql(ex))
        {
            await tx.RollbackAsync(ct);
            return ServiceResult<bool>.Fail("Conflicto al desvincular la etiqueta. Vuelve a consultar.");
        }
    }

    private Task<bool> ExisteActivoAsync(Guid activoId, Guid empresaId, CancellationToken ct) =>
        context.Activos.AsNoTracking().AnyAsync(x =>
            x.EmpresaId == empresaId && x.Id == activoId, ct);

    private async Task<string?> ValidarActivoAsignableAsync(
        Guid activoId, Guid empresaId, CancellationToken ct)
    {
        var activo = await context.Activos.AsNoTracking()
            .Where(x => x.EmpresaId == empresaId && x.Id == activoId)
            .Select(x => new { x.EstadoActivoId }).FirstOrDefaultAsync(ct);
        if (activo is null) return "Activo no encontrado.";
        if (activo.EstadoActivoId == EstadoBaja)
            return "No se pueden vincular etiquetas a un activo dado de baja.";
        return null;
    }

    private async Task<string?> ValidarEtiquetaDisponibleAsync(
        Guid etiquetaId, Guid empresaId, CancellationToken ct)
    {
        var etiqueta = await context.Etiqueta.AsNoTracking()
            .Where(e => e.Id == etiquetaId && e.EmpresaId == empresaId)
            .Select(e => new { e.Anulada }).FirstOrDefaultAsync(ct);
        if (etiqueta is null) return "Etiqueta no encontrada.";
        if (etiqueta.Anulada) return "La etiqueta está anulada y no puede utilizarse.";
        if (await context.ActivoEtiqueta.AsNoTracking().AnyAsync(x =>
                x.EmpresaId == empresaId && x.EtiquetaId == etiquetaId, ct))
            return "La etiqueta ya fue utilizada y no puede volver a vincularse.";
        return null;
    }

    private static void AnularAnterior(
        Etiquetum e, string motivo, DateTime ahora, Guid usuarioId)
    {
        e.Anulada = true;
        e.FechaAnulacion = ahora;
        e.UsuarioAnulacion = usuarioId;
        e.MotivoAnulacion = motivo;
        e.FechaActualizacion = ahora;
        e.UsuarioModificacion = usuarioId;
    }

    private void CompletarUrl(IEnumerable<ActivoEtiquetaHistorialDto> historial)
    {
        foreach (var h in historial)
            h.UrlPublica = EtiquetaUrlHelper.Construir(h.TokenPublico, configuration);
    }

    private static bool EsConflictoSql(Exception ex) =>
        ex is SqlException sql && sql.Number is (1205 or 2601 or 2627 or 547) ||
        ex is DbUpdateException db && db.InnerException is SqlException inner &&
            inner.Number is (1205 or 2601 or 2627 or 547);
}
