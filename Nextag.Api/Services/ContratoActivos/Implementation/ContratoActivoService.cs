using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Constants;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto;
using Nextag.Api.Models.Dto.ContratoActivos;
using Nextag.Api.Services.ContratoActivos.Interface;
using System.Data;
using System.Linq.Expressions;

namespace Nextag.Api.Services.ContratoActivos.Implementation
{
    public class ContratoActivoService : IContratoActivoService
    {
        private readonly NextagContext context;

        private const int EstadoActivoBaja = 7;

        private const int MaximoActivosPorOperacion = 200;

        private const string ContratoNoEncontrado =
            "Contrato no encontrado.";

        private const string ActivoNoEncontrado =
            "Activo no encontrado.";

        private const string AsociacionNoEncontrada =
            "Asociación no encontrada.";

        private const string MensajeConflicto =
            "La información cambió durante la operación. Vuelve a consultar.";

        private static readonly TimeZoneInfo ZonaHoraria =
            TimeZoneInfo.FindSystemTimeZoneById(
                "Central Standard Time (Mexico)");

        public ContratoActivoService(NextagContext context)
        {
            this.context = context;
        }

        private static DateTime ObtenerHoy()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                ZonaHoraria).Date;
        }

        private static readonly Expression<
        Func<ContratoActivo, ContratoActivoDto>> Proyeccion =
        x => new ContratoActivoDto
        {
            Id = x.Id,
            EmpresaId = x.EmpresaId,
            ContratoId = x.ContratoId,
            ActivoId = x.ActivoId,
            ActivoCodigo = x.Activo.Codigo,
            ActivoNombre = x.Activo.Nombre,
            NumeroInventario = x.Activo.NumeroInventario,
            NumeroSerie = x.Activo.NumeroSerie,
            CategoriaActivoId = x.Activo.CategoriaActivoId,
            CategoriaActivoNombre = x.Activo.CategoriaActivo.Nombre,
            EstadoActivoId = x.Activo.EstadoActivoId,
            EstadoActivoNombre = x.Activo.EstadoActivo.Nombre,
            FechaInclusion = x.FechaInclusion,
            FechaExclusion = x.FechaExclusion,
            MotivoExclusion = x.MotivoExclusion,
            Observaciones = x.Observaciones,
            AsociacionVigente = x.FechaExclusion == null,
            FechaCreacion = x.FechaCreacion,
            UsuarioCreacion = x.UsuarioCreacion,
            FechaActualizacion = x.FechaActualizacion,
            UsuarioModificacion = x.UsuarioModificacion
        };

        public async Task<ServiceResult<List<ContratoActivoDto>>> GetAllAsync(
        Guid contratoId,
        Guid empresaId,
        bool incluirHistorial = false,
        CancellationToken ct = default)
        {
            var existeContrato = await context.Contratos
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == contratoId &&
                    x.EmpresaId == empresaId,
                    ct);

            if (!existeContrato)
            {
                return ServiceResult<List<ContratoActivoDto>>
                    .Fail(ContratoNoEncontrado);
            }

            var query = context.ContratoActivos
                .AsNoTracking()
                .Where(x =>
                    x.ContratoId == contratoId &&
                    x.EmpresaId == empresaId);

            if (!incluirHistorial)
            {
                query = query.Where(x =>
                    x.FechaExclusion == null);
            }

            var items = await query
                .OrderByDescending(x => x.FechaExclusion == null)
                .ThenByDescending(x => x.FechaInclusion)
                .ThenByDescending(x => x.Id)
                .Select(Proyeccion)
                .ToListAsync(ct);

            return ServiceResult<List<ContratoActivoDto>>
                .Ok(items);
        }

        public async Task<ServiceResult<ContratoActivoDisponibleListadoDto>>
    GetDisponiblesAsync(
        Guid contratoId,
        Guid empresaId,
        ContratoActivoFiltroDto filtro,
        CancellationToken ct = default)
        {
            var contrato = await context.Contratos
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == contratoId &&
                    x.EmpresaId == empresaId,
                    ct);

            if (contrato is null)
            {
                return ServiceResult<ContratoActivoDisponibleListadoDto>
                    .Fail(ContratoNoEncontrado);
            }

            var error = ValidarContratoParaAsociar(contrato);

            if (error is not null)
            {
                return ServiceResult<ContratoActivoDisponibleListadoDto>
                    .Fail(error);
            }

            var query = context.Activos
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.EstadoActivoId != EstadoActivoBaja &&
                    x.FechaBaja == null &&
                    !x.ContratoActivos.Any(a =>
                        a.EmpresaId == empresaId &&
                        a.ContratoId == contratoId &&
                        a.FechaExclusion == null));

            var buscar = Normalizar(filtro.Buscar);

            if (buscar is not null)
            {
                query = query.Where(x =>
                    x.Codigo.Contains(buscar) ||
                    x.Nombre.Contains(buscar) ||
                    (x.NumeroInventario != null &&
                        x.NumeroInventario.Contains(buscar)) ||
                    (x.NumeroSerie != null &&
                        x.NumeroSerie.Contains(buscar)));
            }

            if (filtro.CategoriaActivoId.HasValue)
            {
                query = query.Where(x =>
                    x.CategoriaActivoId ==
                        filtro.CategoriaActivoId.Value);
            }

            if (filtro.SucursalId.HasValue)
            {
                query = query.Where(x =>
                    x.SucursalId ==
                        filtro.SucursalId.Value);
            }

            if (filtro.UbicacionId.HasValue)
            {
                query = query.Where(x =>
                    x.UbicacionId ==
                        filtro.UbicacionId.Value);
            }

            if (filtro.EstadoActivoId.HasValue)
            {
                query = query.Where(x =>
                    x.EstadoActivoId ==
                        filtro.EstadoActivoId.Value);
            }

            var totalRegistros = await query.CountAsync(ct);

            var pagina = Math.Max(
                filtro.Pagina,
                1);

            var registrosPorPagina = Math.Clamp(
                filtro.RegistrosPorPagina,
                1,
                200);

            var totalPaginas = (int)Math.Ceiling(
                totalRegistros / (double)registrosPorPagina);

            var desplazamiento =
                (long)(pagina - 1) * registrosPorPagina;

            var resultado = new ContratoActivoDisponibleListadoDto
            {
                TotalRegistros = totalRegistros,
                Pagina = pagina,
                RegistrosPorPagina = registrosPorPagina,
                TotalPaginas = totalPaginas
            };

            if (desplazamiento > int.MaxValue ||
                desplazamiento >= totalRegistros)
            {
                return ServiceResult<ContratoActivoDisponibleListadoDto>
                    .Ok(resultado);
            }

            resultado.Items = await query
                .OrderBy(x => x.Nombre)
                .ThenBy(x => x.Codigo)
                .ThenBy(x => x.Id)
                .Skip((int)desplazamiento)
                .Take(registrosPorPagina)
                .Select(x => new ContratoActivoDisponibleDto
                {
                    Id = x.Id,

                    Codigo = x.Codigo,

                    Nombre = x.Nombre,

                    NumeroInventario = x.NumeroInventario,

                    NumeroSerie = x.NumeroSerie,

                    CategoriaActivoId = x.CategoriaActivoId,

                    CategoriaActivoNombre =
                        x.CategoriaActivo.Nombre,

                    SucursalId = x.SucursalId,

                    SucursalNombre =
                        x.Sucursal.Nombre,

                    UbicacionId = x.UbicacionId,

                    UbicacionNombre = x.Ubicacion != null
                        ? x.Ubicacion.Nombre
                        : null,

                    EstadoActivoId = x.EstadoActivoId,

                    EstadoActivoNombre =
                        x.EstadoActivo.Nombre
                })
                .ToListAsync(ct);

            return ServiceResult<ContratoActivoDisponibleListadoDto>
                .Ok(resultado);
        }

        public async Task<ServiceResult<List<ContratoActivoDto>>> AsociarAsync(
    Guid contratoId,
    ContratoActivoAsociarDto dto,
    Guid empresaId,
    Guid usuarioId,
    CancellationToken ct = default)
        {
            if (dto.ActivoIds is null ||
                dto.ActivoIds.Count == 0)
            {
                return ServiceResult<List<ContratoActivoDto>>
                    .Fail("Selecciona al menos un activo.");
            }

            if (dto.ActivoIds.Count > MaximoActivosPorOperacion)
            {
                return ServiceResult<List<ContratoActivoDto>>
                    .Fail("Solo se pueden asociar hasta 200 activos por operación.");
            }

            if (dto.ActivoIds.Any(x => x == Guid.Empty))
            {
                return ServiceResult<List<ContratoActivoDto>>
                    .Fail("Uno o más activos seleccionados no son válidos.");
            }

            var activoIds = dto.ActivoIds
                .Distinct()
                .ToList();

            var observaciones = Normalizar(dto.Observaciones);

            if (observaciones?.Length > 1000)
            {
                return ServiceResult<List<ContratoActivoDto>>
                    .Fail("Las observaciones no pueden exceder 1000 caracteres.");
            }

            var relaciones = new List<ContratoActivo>();

            await using var tx =
                await context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    ct);

            try
            {
                var contrato = await context.Contratos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == contratoId &&
                        x.EmpresaId == empresaId,
                        ct);

                if (contrato is null)
                {
                    return ServiceResult<List<ContratoActivoDto>>
                        .Fail(ContratoNoEncontrado);
                }

                var error = ValidarContratoParaAsociar(contrato);

                if (error is not null)
                {
                    return ServiceResult<List<ContratoActivoDto>>
                        .Fail(error);
                }

                var activos = await context.Activos
                    .AsNoTracking()
                    .Where(x =>
                        x.EmpresaId == empresaId &&
                        activoIds.Contains(x.Id))
                    .Select(x => new
                    {
                        x.Id,
                        x.Codigo,
                        x.Nombre,
                        x.EstadoActivoId,
                        x.FechaBaja
                    })
                    .ToListAsync(ct);

                if (activos.Count != activoIds.Count)
                {
                    return ServiceResult<List<ContratoActivoDto>>
                        .Fail(
                            "Uno o más activos no existen o no pertenecen a la empresa.");
                }

                var activosBaja = activos
                    .Where(x =>
                        x.EstadoActivoId == EstadoActivoBaja ||
                        x.FechaBaja.HasValue)
                    .Select(x => x.Codigo)
                    .ToList();

                if (activosBaja.Count > 0)
                {
                    return ServiceResult<List<ContratoActivoDto>>
                        .Fail(
                            "No se pueden asociar activos dados de baja: " +
                            string.Join(", ", activosBaja));
                }

                var activosYaAsociados = await context.ContratoActivos
                    .AsNoTracking()
                    .Where(x =>
                        x.EmpresaId == empresaId &&
                        x.ContratoId == contratoId &&
                        x.FechaExclusion == null &&
                        activoIds.Contains(x.ActivoId))
                    .Select(x => x.Activo.Codigo)
                    .ToListAsync(ct);

                if (activosYaAsociados.Count > 0)
                {
                    return ServiceResult<List<ContratoActivoDto>>
                        .Fail(
                            "Los siguientes activos ya se encuentran asociados: " +
                            string.Join(", ", activosYaAsociados));
                }

                var ahora = DateTime.UtcNow;

                foreach (var activoId in activoIds)
                {
                    relaciones.Add(new ContratoActivo
                    {
                        EmpresaId = empresaId,

                        ContratoId = contratoId,

                        ActivoId = activoId,

                        FechaInclusion = ahora,

                        FechaExclusion = null,

                        Observaciones = observaciones,

                        FechaCreacion = ahora,

                        UsuarioCreacion = usuarioId
                    });
                }

                context.ContratoActivos.AddRange(relaciones);

                await context.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);
            }
            catch (Exception ex) when (EsConflictoSql(ex))
            {
                // Al salir del método se libera la transacción.
                // Si no se confirmó, será descartada.

                return ServiceResult<List<ContratoActivoDto>>
                    .Fail(MensajeConflicto);
            }

            var idsGenerados = relaciones
                .Select(x => x.Id)
                .ToList();

            var items = await context.ContratoActivos
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.ContratoId == contratoId &&
                    idsGenerados.Contains(x.Id))
                .OrderBy(x => x.Activo.Nombre)
                .ThenBy(x => x.Id)
                .Select(Proyeccion)
                .ToListAsync(ct);

            return ServiceResult<List<ContratoActivoDto>>
                .Ok(items);
        }

        public async Task<ServiceResult<ContratoActivoDto>> ExcluirAsync(
    Guid contratoId,
    Guid id,
    ContratoActivoExcluirDto dto,
    Guid empresaId,
    Guid usuarioId,
    CancellationToken ct = default)
        {
            var motivo = Normalizar(dto.Motivo);

            if (motivo is null)
            {
                return ServiceResult<ContratoActivoDto>
                    .Fail("El motivo de exclusión es obligatorio.");
            }

            if (motivo.Length > 500)
            {
                return ServiceResult<ContratoActivoDto>
                    .Fail(
                        "El motivo de exclusión no puede exceder 500 caracteres.");
            }

            await using var tx =
                await context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    ct);

            try
            {
                var contrato = await context.Contratos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.EmpresaId == empresaId &&
                        x.Id == contratoId,
                        ct);

                if (contrato is null)
                {
                    return ServiceResult<ContratoActivoDto>
                        .Fail(ContratoNoEncontrado);
                }

                var asociacion = await context.ContratoActivos
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        x.ContratoId == contratoId &&
                        x.EmpresaId == empresaId,
                        ct);

                if (asociacion is null)
                {
                    return ServiceResult<ContratoActivoDto>
                        .Fail(AsociacionNoEncontrada);
                }

                if (asociacion.FechaExclusion.HasValue)
                {
                    return ServiceResult<ContratoActivoDto>
                        .Fail("El activo ya fue excluido del contrato.");
                }

                var error = ValidarContratoParaExcluir(contrato);

                if (error is not null)
                {
                    return ServiceResult<ContratoActivoDto>
                        .Fail(error);
                }

                var ahora = DateTime.UtcNow;

                asociacion.FechaExclusion = ahora;

                asociacion.MotivoExclusion = motivo;

                asociacion.FechaActualizacion = ahora;

                asociacion.UsuarioModificacion = usuarioId;

                await context.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);
            }
            catch (Exception ex) when (EsConflictoSql(ex))
            {
                return ServiceResult<ContratoActivoDto>
                    .Fail(MensajeConflicto);
            }

            var detalle = await context.ContratoActivos
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId &&
                    x.ContratoId == contratoId)
                .Select(Proyeccion)
                .FirstOrDefaultAsync(ct);

            return detalle is null
                ? ServiceResult<ContratoActivoDto>
                    .Fail(AsociacionNoEncontrada)
                : ServiceResult<ContratoActivoDto>
                    .Ok(detalle);
        }

        public async Task<ServiceResult<List<ActivoContratoDto>>>
    GetContratosPorActivoAsync(
        Guid activoId,
        Guid empresaId,
        bool incluirHistorial = false,
        CancellationToken ct = default)
        {
            var existeActivo = await context.Activos
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == activoId &&
                    x.EmpresaId == empresaId,
                    ct);

            if (!existeActivo)
            {
                return ServiceResult<List<ActivoContratoDto>>
                    .Fail(ActivoNoEncontrado);
            }

            var query = context.ContratoActivos
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.ActivoId == activoId);

            if (!incluirHistorial)
            {
                query = query.Where(x =>
                    x.FechaExclusion == null);
            }

            var items = await query
                .OrderByDescending(x => x.FechaExclusion == null)
                .ThenByDescending(x => x.FechaInclusion)
                .ThenByDescending(x => x.Id)
                .Select(x => new ActivoContratoDto
                {
                    Id = x.Id,

                    ActivoId = x.ActivoId,

                    ContratoId = x.ContratoId,

                    NumeroContrato =
                        x.Contrato.NumeroContrato,

                    ContratoNombre =
                        x.Contrato.Nombre,

                    TipoContratoId =
                        x.Contrato.TipoContratoId,

                    TipoContratoNombre =
                        x.Contrato.TipoContrato.Nombre,

                    EstadoContratoId =
                        x.Contrato.EstadoContratoId,

                    EstadoContratoClave =
                        x.Contrato.EstadoContrato.Clave,

                    EstadoContratoNombre =
                        x.Contrato.EstadoContrato.Nombre,

                    ClienteId =
                        x.Contrato.ClienteId,

                    ClienteNombre =
                        x.Contrato.Cliente != null
                            ? x.Contrato.Cliente.Nombre
                            : null,

                    ProveedorId =
                        x.Contrato.ProveedorId,

                    ProveedorNombre =
                        x.Contrato.Proveedor != null
                            ? x.Contrato.Proveedor.Nombre
                            : null,

                    FechaInicioContrato =
                        x.Contrato.FechaInicio,

                    FechaFinContrato =
                        x.Contrato.FechaFin,

                    ContratoActivo =
                        x.Contrato.Activo == true,

                    FechaInclusion =
                        x.FechaInclusion,

                    FechaExclusion =
                        x.FechaExclusion,

                    MotivoExclusion =
                        x.MotivoExclusion,

                    Observaciones =
                        x.Observaciones,

                    AsociacionVigente =
                        x.FechaExclusion == null
                })
                .ToListAsync(ct);

            var hoy = ObtenerHoy();

            foreach (var item in items)
            {
                var estadoOperativo =
                    item.EstadoContratoId ==
                        EstadosContratoEnum.Vigente ||
                    item.EstadoContratoId ==
                        EstadosContratoEnum.Suspendido;

                item.ContratoVencido =
                    item.ContratoActivo &&
                    item.FechaFinContrato.HasValue &&
                    item.FechaFinContrato.Value.Date < hoy &&
                    (
                        estadoOperativo ||
                        item.EstadoContratoId ==
                            EstadosContratoEnum.Vencido
                    );

                item.CoberturaVigente =
                    item.AsociacionVigente &&
                    item.ContratoActivo &&
                    item.EstadoContratoId ==
                        EstadosContratoEnum.Vigente &&
                    item.FechaInicioContrato.Date <= hoy &&
                    (
                        !item.FechaFinContrato.HasValue ||
                        item.FechaFinContrato.Value.Date >= hoy
                    );
            }

            return ServiceResult<List<ActivoContratoDto>>
                .Ok(items);
        }

        private static int ObtenerEstadoEfectivo(Contrato contrato)
        {
            var estado = contrato.EstadoContratoId;

            var hoy = ObtenerHoy();

            if (contrato.FechaFin.HasValue &&
                contrato.FechaFin.Value.Date < hoy &&
                (
                    estado == EstadosContratoEnum.Vigente ||
                    estado == EstadosContratoEnum.Suspendido
                ))
            {
                return EstadosContratoEnum.Vencido;
            }

            return estado;
        }

        private static string? ValidarContratoParaAsociar(Contrato contrato)
        {
            if (contrato.Activo != true)
            {
                return "No se pueden asociar activos a un contrato dado de baja.";
            }

            var estado = ObtenerEstadoEfectivo(contrato);

            if (estado != EstadosContratoEnum.Borrador &&
                estado != EstadosContratoEnum.Vigente)
            {
                return "Solo se pueden asociar activos a contratos en borrador o vigentes.";
            }

            return null;
        }

        private static string? ValidarContratoParaExcluir(Contrato contrato)
        {
            if (contrato.Activo != true)
            {
                return "No se pueden excluir activos de un contrato dado de baja.";
            }

            var estado = ObtenerEstadoEfectivo(contrato);

            if (estado != EstadosContratoEnum.Borrador &&
                estado != EstadosContratoEnum.Vigente &&
                estado != EstadosContratoEnum.Suspendido)
            {
                return "No se pueden excluir activos en el estado actual del contrato.";
            }

            return null;
        }

        private static string? Normalizar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? null
                : valor.Trim();
        }

        private static bool EsConflictoSql(Exception ex)
        {
            if (ex is SqlException sql)
            {
                return sql.Number is
                    1205 or
                    2601 or
                    2627 or
                    547;
            }

            if (ex is DbUpdateException db &&
                db.InnerException is SqlException inner)
            {
                return inner.Number is
                    1205 or
                    2601 or
                    2627 or
                    547;
            }

            return false;
        }
    }
}
