using System.Data;
using System.Linq.Expressions;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Helpers;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto;
using Nextag.Api.Models.Dto.Etiquetas;
using Nextag.Api.Services.Etiquetas.Interface;

namespace Nextag.Api.Services.Etiquetas.Implementation;

public class EtiquetaService : IEtiquetaService
{
    private readonly NextagContext context;
    private readonly IConfiguration configuration;

    public EtiquetaService(NextagContext context, IConfiguration configuration)
    {
        this.context = context;
        this.configuration = configuration;
    }

    // La proyección usa las propiedades del scaffold: Etiquetum y ActivoEtiquetum.
    // Sin carga de entidades fuera del ámbito de la empresa autenticada.
    private static readonly Expression<Func<Etiquetum, EtiquetaDto>> Proyeccion = e => new EtiquetaDto
    {
        Id = e.Id,
        EmpresaId = e.EmpresaId,
        Codigo = e.Codigo,
        TokenPublico = e.TokenPublico,
        UidNfc = e.UidNfc,
        TieneNfc = e.TieneNfc == true,
        TieneQr = e.TieneQr == true,
        EsAntimetal = e.EsAntimetal,
        NfcBloqueado = e.NfcBloqueado,
        Anulada = e.Anulada,
        FechaAnulacion = e.FechaAnulacion,
        MotivoAnulacion = e.MotivoAnulacion,
        Estatus = e.Anulada ||
                  (e.ActivoEtiquetum != null && e.ActivoEtiquetum.FechaDesvinculacion != null)
            ? "RETIRADA"
            : e.ActivoEtiquetum != null ? "VINCULADA" : "DISPONIBLE",
        ActivoId = e.ActivoEtiquetum != null &&
                   e.ActivoEtiquetum.FechaDesvinculacion == null
            ? e.ActivoEtiquetum.ActivoId : null,
        ActivoCodigo = e.ActivoEtiquetum != null &&
                       e.ActivoEtiquetum.FechaDesvinculacion == null
            ? e.ActivoEtiquetum.Activo.Codigo : null,
        FechaCreacion = e.FechaCreacion,
        UsuarioCreacion = e.UsuarioCreacion,
        FechaActualizacion = e.FechaActualizacion,
        UsuarioModificacion = e.UsuarioModificacion
    };

    public async Task<EtiquetaPaginaDto> GetAllAsync(
        Guid empresaId, EtiquetaFiltroDto filtros, CancellationToken ct = default)
    {
        var query = context.Etiqueta.AsNoTracking().Where(e => e.EmpresaId == empresaId);
        var buscar = filtros.Buscar?.Trim();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            query = query.Where(e => e.Codigo.Contains(buscar) ||
                (e.UidNfc != null && e.UidNfc.Contains(buscar)));
        }
        if (filtros.TieneNfc.HasValue)
            query = query.Where(e => e.TieneNfc == filtros.TieneNfc.Value);
        if (filtros.TieneQr.HasValue)
            query = query.Where(e => e.TieneQr == filtros.TieneQr.Value);

        switch (filtros.Estatus?.Trim().ToUpperInvariant())
        {
            case "DISPONIBLE":
                query = query.Where(e => !e.Anulada && e.ActivoEtiquetum == null);
                break;
            case "VINCULADA":
                query = query.Where(e => !e.Anulada && e.ActivoEtiquetum != null &&
                    e.ActivoEtiquetum.FechaDesvinculacion == null);
                break;
            case "RETIRADA":
                query = query.Where(e => e.Anulada ||
                    (e.ActivoEtiquetum != null && e.ActivoEtiquetum.FechaDesvinculacion != null));
                break;
        }

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(e => e.FechaCreacion)
            .ThenBy(e => e.Id)
            .Skip((filtros.Pagina - 1) * filtros.TamanoPagina)
            .Take(filtros.TamanoPagina)
            .Select(Proyeccion)
            .ToListAsync(ct);
        CompletarUrl(items);

        return new EtiquetaPaginaDto
        {
            Items = items,
            Total = total,
            Pagina = filtros.Pagina,
            TamanoPagina = filtros.TamanoPagina
        };
    }

    public async Task<EtiquetaDto?> GetByIdAsync(
        Guid id, Guid empresaId, CancellationToken ct = default)
    {
        var etiqueta = await context.Etiqueta.AsNoTracking()
            .Where(e => e.Id == id && e.EmpresaId == empresaId)
            .Select(Proyeccion).FirstOrDefaultAsync(ct);
        if (etiqueta is not null) CompletarUrl(new[] { etiqueta });
        return etiqueta;
    }

    // Genera etiquetas listas para imprimir; no registra todavía UID ni bloqueo físico.
    // Código aleatorio global: NT- + 20 caracteres hex (80 bits).
    // Token público aleatorio: 64 caracteres hex (256 bits). No contiene el GUID del activo.
    public async Task<ServiceResult<List<EtiquetaDto>>> GenerarAsync(
        EtiquetaGenerarDto dto, Guid empresaId, Guid usuarioId,
        CancellationToken ct = default)
    {
        if (dto.Cantidad < 1 || dto.Cantidad > 250)
            return ServiceResult<List<EtiquetaDto>>.Fail("La cantidad debe estar entre 1 y 250.");
        if (!dto.TieneNfc && !dto.TieneQr)
            return ServiceResult<List<EtiquetaDto>>.Fail("La etiqueta debe contener NFC, QR o ambos.");
        if (dto.EsAntimetal && !dto.TieneNfc)
            return ServiceResult<List<EtiquetaDto>>.Fail(
                "La opción antimetal se utiliza con etiquetas NFC.");

        var ahora = DateTime.UtcNow;
        var generadas = new List<Etiquetum>(dto.Cantidad);
        for (var i = 0; i < dto.Cantidad; i++)
        {
            generadas.Add(new Etiquetum
            {
                EmpresaId = empresaId,
                Codigo = $"NT-{Convert.ToHexString(RandomNumberGenerator.GetBytes(10))}",
                TokenPublico = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant(),
                TieneNfc = dto.TieneNfc,
                TieneQr = dto.TieneQr,
                EsAntimetal = dto.EsAntimetal,
                NfcBloqueado = false,
                Anulada = false,
                FechaCreacion = ahora,
                UsuarioCreacion = usuarioId
            });
        }

        context.Etiqueta.AddRange(generadas);
        try
        {
            // Un SaveChanges aplica los inserts de este lote de forma atómica.
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (EsConflictoSql(ex))
        {
            return ServiceResult<List<EtiquetaDto>>.Fail(
                "No fue posible generar el lote por un conflicto de identificadores. Intenta nuevamente.");
        }

        var items = generadas.Select(e => new EtiquetaDto
        {
            Id = e.Id,
            EmpresaId = e.EmpresaId,
            Codigo = e.Codigo,
            TokenPublico = e.TokenPublico,
            TieneNfc = e.TieneNfc == true,
            TieneQr = e.TieneQr == true,
            EsAntimetal = e.EsAntimetal,
            NfcBloqueado = false,
            Anulada = false,
            Estatus = "DISPONIBLE",
            FechaCreacion = e.FechaCreacion,
            UsuarioCreacion = e.UsuarioCreacion
        }).ToList();
        CompletarUrl(items);
        return ServiceResult<List<EtiquetaDto>>.Ok(items);
    }

    // Solo registra el UID y la CONFIRMACIÓN MANUAL del bloqueo.
    // Es indispensable verificar la escritura/bloqueo en el chip con un lector NFC.
    public async Task<ServiceResult<EtiquetaDto>> ActualizarNfcAsync(
        Guid id, EtiquetaNfcDto dto, Guid empresaId, Guid usuarioId,
        CancellationToken ct = default)
    {
        var etiqueta = await context.Etiqueta.FirstOrDefaultAsync(e =>
            e.Id == id && e.EmpresaId == empresaId, ct);
        if (etiqueta is null)
            return ServiceResult<EtiquetaDto>.Fail("Etiqueta no encontrada.");
        if (etiqueta.Anulada)
            return ServiceResult<EtiquetaDto>.Fail("No se puede modificar una etiqueta anulada.");
        if (etiqueta.TieneNfc != true)
            return ServiceResult<EtiquetaDto>.Fail("La etiqueta no tiene NFC.");
        if (dto.UidNfc is null && !dto.NfcBloqueado.HasValue)
            return ServiceResult<EtiquetaDto>.Fail("Indica el UID o el estado de bloqueo NFC.");
        if (etiqueta.NfcBloqueado && dto.NfcBloqueado == false)
            return ServiceResult<EtiquetaDto>.Fail("No se puede revertir un bloqueo NFC confirmado.");
        if (dto.NfcBloqueado == true && !etiqueta.NfcBloqueado &&
            !dto.ConfirmoVerificacionFisica)
            return ServiceResult<EtiquetaDto>.Fail(
                "Confirma que comprobaste físicamente el bloqueo de escritura del chip.");

        if (dto.UidNfc is not null)
        {
            var uid = dto.UidNfc.Trim().ToUpperInvariant();
            if (uid.Length == 0)
                return ServiceResult<EtiquetaDto>.Fail("El UID NFC no puede estar vacío.");
            if (etiqueta.NfcBloqueado && etiqueta.UidNfc != uid)
                return ServiceResult<EtiquetaDto>.Fail("No puedes cambiar el UID de una etiqueta con NFC bloqueado.");
            if (await context.Etiqueta.AsNoTracking().AnyAsync(e =>
                    e.EmpresaId == empresaId && e.Id != id && e.UidNfc == uid, ct))
                return ServiceResult<EtiquetaDto>.Fail("Ya existe otra etiqueta con ese UID en la empresa.");
            etiqueta.UidNfc = uid;
        }
        if (dto.NfcBloqueado.HasValue)
            etiqueta.NfcBloqueado = dto.NfcBloqueado.Value;
        etiqueta.FechaActualizacion = DateTime.UtcNow;
        etiqueta.UsuarioModificacion = usuarioId;
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (EsConflictoSql(ex))
        {
            return ServiceResult<EtiquetaDto>.Fail("No se pudieron actualizar los datos NFC.");
        }
        var actualizado = await GetByIdAsync(id, empresaId, ct);
        return ServiceResult<EtiquetaDto>.Ok(actualizado!);
    }

    // Anulación explícita: solo es válida cuando no existe vínculo vigente.
    public async Task<ServiceResult<bool>> AnularAsync(
        Guid id, EtiquetaAnularDto dto, Guid empresaId, Guid usuarioId,
        CancellationToken ct = default)
    {
        var motivo = dto.Motivo?.Trim();
        if (string.IsNullOrWhiteSpace(motivo))
            return ServiceResult<bool>.Fail("Indica el motivo de anulación.");

        await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            var etiqueta = await context.Etiqueta.FirstOrDefaultAsync(e =>
                e.Id == id && e.EmpresaId == empresaId, ct);
            if (etiqueta is null)
                return ServiceResult<bool>.Fail("Etiqueta no encontrada.");
            if (etiqueta.Anulada)
                return ServiceResult<bool>.Ok(true);

            var tieneVinculo = await context.ActivoEtiqueta.AsNoTracking().AnyAsync(x =>
                x.EmpresaId == empresaId && x.EtiquetaId == id &&
                x.FechaDesvinculacion == null, ct);
            if (tieneVinculo)
                return ServiceResult<bool>.Fail(
                    "La etiqueta está vinculada. Desvincúlala o reemplázala desde el activo.");

            var ahora = DateTime.UtcNow;
            etiqueta.Anulada = true;
            etiqueta.FechaAnulacion = ahora;
            etiqueta.UsuarioAnulacion = usuarioId;
            etiqueta.MotivoAnulacion = motivo;
            etiqueta.FechaActualizacion = ahora;
            etiqueta.UsuarioModificacion = usuarioId;
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception ex) when (EsConflictoSql(ex))
        {
            await tx.RollbackAsync(ct);
            return ServiceResult<bool>.Fail("Conflicto al anular la etiqueta. Intenta de nuevo.");
        }
    }

    private void CompletarUrl(IEnumerable<EtiquetaDto> etiquetas)
    {
        foreach (var e in etiquetas)
            e.UrlPublica = EtiquetaUrlHelper.Construir(e.TokenPublico, configuration);
    }

    private static bool EsConflictoSql(Exception ex) =>
        ex is SqlException sql && sql.Number is (1205 or 2601 or 2627 or 547) ||
        ex is DbUpdateException db && db.InnerException is SqlException inner &&
            inner.Number is (1205 or 2601 or 2627 or 547);
}
