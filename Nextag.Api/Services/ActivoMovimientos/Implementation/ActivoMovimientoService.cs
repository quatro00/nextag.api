using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto.ActivoMovimientos;
using Nextag.Api.Models.Dto;
using Nextag.Api.Services.ActivoMovimientos.Interface;
using System.Data;

namespace Nextag.Api.Services.ActivoMovimientos.Implementation
{
    public class ActivoMovimientoService : IActivoMovimientoService
    {
        private const int EstadoBaja = 7;

        private readonly NextagContext context;

        public ActivoMovimientoService(NextagContext context)
        {
            this.context = context;
        }

        // ==================================================
        // PROYECCION BASE
        // ==================================================

        private IQueryable<ActivoMovimientoDto> Consulta(Guid empresaId)
        {
            return context.ActivoMovimientos
                .AsNoTracking()
                .Where(x => x.EmpresaId == empresaId)
                .Select(x => new ActivoMovimientoDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,

                    ActivoId = x.ActivoId,
                    ActivoCodigo = x.Activo.Codigo,
                    ActivoNombre = x.Activo.Nombre,
                    ActivoNumeroSerie = x.Activo.NumeroSerie,

                    TipoMovimientoActivoId = x.TipoMovimientoActivoId,
                    TipoMovimientoClave = x.TipoMovimientoActivo.Clave,
                    TipoMovimientoNombre = x.TipoMovimientoActivo.Nombre,

                    FechaMovimiento = x.FechaMovimiento,
                    Motivo = x.Motivo,
                    Observaciones = x.Observaciones,

                    SucursalOrigenId = x.SucursalOrigenId,
                    UbicacionOrigenId = x.UbicacionOrigenId,
                    SucursalOrigenNombre = x.SucursalOrigenNombre,
                    UbicacionOrigenNombre = x.UbicacionOrigenNombre,

                    EsOrigenExterno = x.EsOrigenExterno,
                    OrigenExternoNombre = x.OrigenExternoNombre,
                    OrigenExternoDireccion = x.OrigenExternoDireccion,

                    SucursalDestinoId = x.SucursalDestinoId,
                    UbicacionDestinoId = x.UbicacionDestinoId,
                    SucursalDestinoNombre = x.SucursalDestinoNombre,
                    UbicacionDestinoNombre = x.UbicacionDestinoNombre,

                    EsDestinoExterno = x.EsDestinoExterno,
                    DestinoExternoNombre = x.DestinoExternoNombre,
                    DestinoExternoDireccion = x.DestinoExternoDireccion,
                    ProveedorDestinoId = x.ProveedorDestinoId,

                    MovimientoOrigenId = x.MovimientoOrigenId,

                    EstadoActivoOrigenId = x.EstadoActivoOrigenId,
                    EstadoActivoDestinoId = x.EstadoActivoDestinoId,

                    TipoReferencia = x.TipoReferencia,
                    ReferenciaId = x.ReferenciaId,

                    Anulado = x.Anulado,
                    FechaAnulacion = x.FechaAnulacion,
                    UsuarioAnulacion = x.UsuarioAnulacion,
                    MotivoAnulacion = x.MotivoAnulacion,

                    FechaCreacion = x.FechaCreacion,
                    UsuarioCreacion = x.UsuarioCreacion
                });
        }

        // ==================================================
        // LISTADO PAGINADO
        // ==================================================

        public async Task<ActivoMovimientoPaginaDto> GetAllAsync(
            Guid empresaId,
            ActivoMovimientoFiltroDto filtros,
            CancellationToken ct = default)
        {
            var pagina = Math.Max(1, filtros.Pagina);
            var tamano = Math.Clamp(filtros.TamanoPagina, 1, 100);

            var query = Consulta(empresaId);

            if (filtros.ActivoId.HasValue)
            {
                query = query.Where(x =>
                    x.ActivoId == filtros.ActivoId.Value);
            }

            if (filtros.TipoMovimientoActivoId.HasValue)
            {
                query = query.Where(x =>
                    x.TipoMovimientoActivoId ==
                    filtros.TipoMovimientoActivoId.Value);
            }

            if (filtros.SucursalId.HasValue)
            {
                query = query.Where(x =>
                    x.SucursalOrigenId == filtros.SucursalId.Value ||
                    x.SucursalDestinoId == filtros.SucursalId.Value);
            }

            if (filtros.FechaDesde.HasValue)
            {
                query = query.Where(x =>
                    x.FechaMovimiento >= filtros.FechaDesde.Value);
            }

            if (filtros.FechaHasta.HasValue)
            {
                query = query.Where(x =>
                    x.FechaMovimiento <= filtros.FechaHasta.Value);
            }

            if (filtros.EsDestinoExterno.HasValue)
            {
                query = query.Where(x =>
                    x.EsDestinoExterno == filtros.EsDestinoExterno.Value);
            }

            if (filtros.Anulado.HasValue)
            {
                query = query.Where(x =>
                    x.Anulado == filtros.Anulado.Value);
            }

            if (!string.IsNullOrWhiteSpace(filtros.Buscar))
            {
                var buscar = filtros.Buscar.Trim();

                query = query.Where(x =>
                    x.ActivoCodigo.Contains(buscar) ||
                    x.ActivoNombre.Contains(buscar) ||
                    (
                        x.ActivoNumeroSerie != null &&
                        x.ActivoNumeroSerie.Contains(buscar)
                    ));
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.FechaMovimiento)
                .ThenByDescending(x => x.Id)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync(ct);

            return new ActivoMovimientoPaginaDto
            {
                Items = items,
                Total = total,
                Pagina = pagina,
                TamanoPagina = tamano,
                TotalPaginas =
                    (int)Math.Ceiling(total / (double)tamano)
            };
        }

        // ==================================================
        // DETALLE
        // ==================================================

        public Task<ActivoMovimientoDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default)
        {
            return Consulta(empresaId)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        // ==================================================
        // HISTORIAL
        // ==================================================

        public Task<List<ActivoMovimientoDto>> GetHistorialActivoAsync(
            Guid activoId,
            Guid empresaId,
            CancellationToken ct = default)
        {
            return Consulta(empresaId)
                .Where(x => x.ActivoId == activoId)
                .OrderByDescending(x => x.FechaMovimiento)
                .ThenByDescending(x => x.Id)
                .ToListAsync(ct);
        }

        // ==================================================
        // CATALOGO
        // ==================================================

        public Task<List<TipoMovimientoActivoDto>> GetTiposAsync(
            CancellationToken ct = default)
        {
            return context.TipoMovimientoActivos
                .AsNoTracking()
                .Where(x => x.Activo == true)
                .OrderBy(x => x.Id)
                .Select(x => new TipoMovimientoActivoDto
                {
                    Id = x.Id,
                    Clave = x.Clave,
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion
                })
                .ToListAsync(ct);
        }

        // ==================================================
        // SITUACION FISICA
        // ==================================================

        public async Task<ActivoSituacionDto?> GetSituacionAsync(
            Guid activoId,
            Guid empresaId,
            CancellationToken ct = default)
        {
            var activo = await context.Activos
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.Id == activoId)
                .Select(x => new ActivoSituacionDto
                {
                    ActivoId = x.Id,
                    ActivoCodigo = x.Codigo,
                    ActivoNombre = x.Nombre,

                    EstadoActivoId = x.EstadoActivoId,
                    EstadoActivoNombre = x.EstadoActivo.Nombre,

                    SucursalId = x.SucursalId,
                    SucursalNombre = x.Sucursal.Nombre,

                    UbicacionId = x.UbicacionId,
                    UbicacionNombre =
                        x.Ubicacion != null
                            ? x.Ubicacion.Nombre
                            : null
                })
                .FirstOrDefaultAsync(ct);

            if (activo == null)
            {
                return null;
            }

            var salidaAbierta = await ObtenerSalidaAbiertaAsync(
                empresaId,
                activoId,
                ct);

            if (salidaAbierta != null)
            {
                activo.EstaFuera = true;

                activo.UbicacionExternaNombre =
                    salidaAbierta.DestinoExternoNombre;

                activo.UbicacionExternaDireccion =
                    salidaAbierta.DestinoExternoDireccion;

                activo.ProveedorExternoId =
                    salidaAbierta.ProveedorDestinoId;

                activo.MovimientoSalidaId =
                    salidaAbierta.Id;

                activo.FechaSalida =
                    salidaAbierta.FechaMovimiento;
            }

            return activo;
        }

        // ==================================================
        // OBTENER SALIDA ABIERTA
        // ==================================================

        private async Task<ActivoMovimiento?> ObtenerSalidaAbiertaAsync(
            Guid empresaId,
            Guid activoId,
            CancellationToken ct)
        {
            // En esta versión una salida permanece abierta
            // hasta que exista un retorno válido asociado.

            return await context.ActivoMovimientos
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.ActivoId == activoId &&
                    !x.Anulado &&
                    x.EsDestinoExterno &&
                    !context.ActivoMovimientos.Any(r =>
                        r.EmpresaId == empresaId &&
                        r.ActivoId == activoId &&
                        !r.Anulado &&
                        r.MovimientoOrigenId == x.Id))
                .OrderByDescending(x => x.FechaMovimiento)
                .ThenByDescending(x => x.Id)
                .FirstOrDefaultAsync(ct);
        }

        // ==================================================
        // REGISTRAR MOVIMIENTO
        // ==================================================

        public async Task<ServiceResult<ActivoMovimientoDto>> CreateAsync(
            ActivoMovimientoCrearDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default)
        {
            if (dto.ActivoId == Guid.Empty)
            {
                return ServiceResult<ActivoMovimientoDto>
                    .Fail("Selecciona un activo.");
            }

            var tipo = await context.TipoMovimientoActivos
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.TipoMovimientoActivoId &&
                    x.Activo == true,
                    ct);

            if (tipo == null)
            {
                return ServiceResult<ActivoMovimientoDto>
                    .Fail("El tipo de movimiento no es válido.");
            }

            var esSalida = tipo.Clave is
                "SALIDA_TEMPORAL" or "ENVIO_MANTENIMIENTO";

            var esRetorno = tipo.Clave is
                "RETORNO" or "REGRESO_MANTENIMIENTO";

            var esInterno = tipo.Clave is
                "TRASLADO" or "AJUSTE_UBICACION";

            if (!esSalida && !esRetorno && !esInterno)
            {
                return ServiceResult<ActivoMovimientoDto>
                    .Fail("Tipo de movimiento no soportado.");
            }

            // Las referencias futuras todavía no se reciben.
            // Se habilitarán al implementar soporte/mantenimiento.
            if (dto.TipoReferencia != null || dto.ReferenciaId.HasValue)
            {
                return ServiceResult<ActivoMovimientoDto>
                    .Fail(
                        "Las referencias a tickets y mantenimientos " +
                        "se habilitarán al implementar esos módulos.");
            }

            await using var transaction =
                await context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    ct);

            try
            {
                var activo = await context.Activos
                    .FirstOrDefaultAsync(x =>
                        x.EmpresaId == empresaId &&
                        x.Id == dto.ActivoId,
                        ct);

                if (activo == null)
                {
                    return ServiceResult<ActivoMovimientoDto>
                        .Fail("El activo no existe en la empresa.");
                }

                if (activo.EstadoActivoId == EstadoBaja ||
                    activo.FechaBaja.HasValue)
                {
                    return ServiceResult<ActivoMovimientoDto>
                        .Fail(
                            "No se permiten movimientos de activos dados de baja.");
                }

                var salidaAbierta = await ObtenerSalidaAbiertaAsync(
                    empresaId,
                    activo.Id,
                    ct);

                if (salidaAbierta != null && !esRetorno)
                {
                    return ServiceResult<ActivoMovimientoDto>
                        .Fail(
                            "El activo se encuentra fuera de las instalaciones. " +
                            "Debes registrar primero su retorno.");
                }

                if (salidaAbierta == null && esRetorno)
                {
                    return ServiceResult<ActivoMovimientoDto>
                        .Fail(
                            "El activo no tiene una salida externa pendiente.");
                }

                // ==========================================
                // VALIDAR DESTINO
                // ==========================================

                Sucursal? sucursalDestino = null;
                Ubicacion? ubicacionDestino = null;
                Proveedor? proveedorDestino = null;

                if (esSalida)
                {
                    if (dto.SucursalDestinoId.HasValue ||
                        dto.UbicacionDestinoId.HasValue)
                    {
                        return ServiceResult<ActivoMovimientoDto>
                            .Fail(
                                "Una salida externa no puede tener destino interno.");
                    }

                    if (string.IsNullOrWhiteSpace(
                        dto.DestinoExternoNombre))
                    {
                        return ServiceResult<ActivoMovimientoDto>
                            .Fail(
                                "Indica el destino externo del activo.");
                    }

                    if (dto.ProveedorDestinoId.HasValue)
                    {
                        proveedorDestino = await context.Proveedors
                            .FirstOrDefaultAsync(x =>
                                x.EmpresaId == empresaId &&
                                x.Id == dto.ProveedorDestinoId.Value,
                                ct);

                        if (proveedorDestino == null ||
                            proveedorDestino.Activo != true)
                        {
                            return ServiceResult<ActivoMovimientoDto>
                                .Fail(
                                    "El proveedor destino no existe o está inactivo.");
                        }
                    }
                }
                else
                {
                    if (!dto.SucursalDestinoId.HasValue)
                    {
                        return ServiceResult<ActivoMovimientoDto>
                            .Fail("Selecciona la sucursal destino.");
                    }

                    if (!string.IsNullOrWhiteSpace(
                        dto.DestinoExternoNombre) ||
                        !string.IsNullOrWhiteSpace(
                            dto.DestinoExternoDireccion) ||
                        dto.ProveedorDestinoId.HasValue)
                    {
                        return ServiceResult<ActivoMovimientoDto>
                            .Fail(
                                "Un movimiento interno no debe incluir destino externo.");
                    }

                    sucursalDestino = await context.Sucursals
                        .FirstOrDefaultAsync(x =>
                            x.EmpresaId == empresaId &&
                            x.Id == dto.SucursalDestinoId.Value,
                            ct);

                    if (sucursalDestino == null ||
                        sucursalDestino.Activo != true)
                    {
                        return ServiceResult<ActivoMovimientoDto>
                            .Fail(
                                "La sucursal destino no existe o está inactiva.");
                    }

                    if (dto.UbicacionDestinoId.HasValue)
                    {
                        ubicacionDestino = await context.Ubicacions
                            .FirstOrDefaultAsync(x =>
                                x.EmpresaId == empresaId &&
                                x.SucursalId == sucursalDestino.Id &&
                                x.Id == dto.UbicacionDestinoId.Value,
                                ct);

                        if (ubicacionDestino == null ||
                            ubicacionDestino.Activo != true)
                        {
                            return ServiceResult<ActivoMovimientoDto>
                                .Fail(
                                    "La ubicación destino no pertenece " +
                                    "a la sucursal seleccionada o está inactiva.");
                        }
                    }
                }

                // ==========================================
                // VALIDACIONES DE RETORNO
                // ==========================================

                if (esRetorno)
                {
                    if (salidaAbierta == null ||
                        !dto.MovimientoOrigenId.HasValue ||
                        salidaAbierta.Id != dto.MovimientoOrigenId.Value)
                    {
                        return ServiceResult<ActivoMovimientoDto>
                            .Fail(
                                "Selecciona la salida externa pendiente que corresponde al retorno.");
                    }

                    var tipoEsperado =
                        tipo.Clave == "RETORNO"
                            ? "SALIDA_TEMPORAL"
                            : "ENVIO_MANTENIMIENTO";

                    var tipoSalida = await context.TipoMovimientoActivos
                        .AsNoTracking()
                        .Where(x =>
                            x.Id == salidaAbierta.TipoMovimientoActivoId)
                        .Select(x => x.Clave)
                        .FirstAsync(ct);

                    if (tipoSalida != tipoEsperado)
                    {
                        return ServiceResult<ActivoMovimientoDto>
                            .Fail(
                                "El tipo de retorno no corresponde " +
                                "con la salida registrada.");
                    }

                    var yaRetornado = await context.ActivoMovimientos
                        .AnyAsync(x =>
                            x.EmpresaId == empresaId &&
                            x.ActivoId == activo.Id &&
                            x.MovimientoOrigenId == salidaAbierta.Id &&
                            !x.Anulado,
                            ct);

                    if (yaRetornado)
                    {
                        return ServiceResult<ActivoMovimientoDto>
                            .Fail(
                                "Esta salida ya tiene un retorno registrado.");
                    }
                }
                else if (dto.MovimientoOrigenId.HasValue)
                {
                    return ServiceResult<ActivoMovimientoDto>
                        .Fail(
                            "Solo los retornos pueden indicar un movimiento de origen.");
                }

                // ==========================================
                // VALIDAR CAMBIO INTERNO
                // ==========================================

                if (esInterno &&
                    activo.SucursalId == sucursalDestino!.Id &&
                    activo.UbicacionId == dto.UbicacionDestinoId)
                {
                    return ServiceResult<ActivoMovimientoDto>
                        .Fail(
                            "El activo ya se encuentra en la ubicación seleccionada.");
                }

                // ==========================================
                // ESTADO DESTINO
                // ==========================================

                var estadoDestinoId =
                    dto.EstadoActivoDestinoId ?? activo.EstadoActivoId;

                if (estadoDestinoId == EstadoBaja)
                {
                    return ServiceResult<ActivoMovimientoDto>
                        .Fail(
                            "La baja del activo no debe registrarse " +
                            "mediante un movimiento ordinario.");
                }

                var estadoValido = await context.EstadoActivos
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == estadoDestinoId &&
                        x.Activo == true,
                        ct);

                if (!estadoValido)
                {
                    return ServiceResult<ActivoMovimientoDto>
                        .Fail(
                            "El estado destino no es válido o está inactivo.");
                }

                // ==========================================
                // UBICACION ORIGEN
                // ==========================================

                var sucursalOrigen = await context.Sucursals
                    .AsNoTracking()
                    .Where(x =>
                        x.EmpresaId == empresaId &&
                        x.Id == activo.SucursalId)
                    .Select(x => x.Nombre)
                    .FirstOrDefaultAsync(ct);

                string? ubicacionOrigen = null;

                if (activo.UbicacionId.HasValue)
                {
                    ubicacionOrigen = await context.Ubicacions
                        .AsNoTracking()
                        .Where(x =>
                            x.EmpresaId == empresaId &&
                            x.SucursalId == activo.SucursalId &&
                            x.Id == activo.UbicacionId.Value)
                        .Select(x => x.Nombre)
                        .FirstOrDefaultAsync(ct);
                }

                // ==========================================
                // CREAR MOVIMIENTO
                // ==========================================

                var ahora = DateTime.UtcNow;

                var movimiento = new ActivoMovimiento
                {
                    EmpresaId = empresaId,
                    ActivoId = activo.Id,
                    TipoMovimientoActivoId = tipo.Id,
                    FechaMovimiento = ahora,

                    Motivo = Normalizar(dto.Motivo),
                    Observaciones = Normalizar(dto.Observaciones),

                    EsOrigenExterno = esRetorno,
                    EsDestinoExterno = esSalida,

                    EstadoActivoOrigenId = activo.EstadoActivoId,
                    EstadoActivoDestinoId = estadoDestinoId,

                    MovimientoOrigenId =
                        esRetorno ? salidaAbierta!.Id : null,

                    FechaCreacion = ahora,
                    UsuarioCreacion = usuarioId
                };

                if (esRetorno)
                {
                    // El origen físico es el taller o destino
                    // externo de la salida anterior.

                    movimiento.OrigenExternoNombre =
                        salidaAbierta!.DestinoExternoNombre;

                    movimiento.OrigenExternoDireccion =
                        salidaAbierta.DestinoExternoDireccion;
                }
                else
                {
                    movimiento.SucursalOrigenId =
                        activo.SucursalId;

                    movimiento.UbicacionOrigenId =
                        activo.UbicacionId;

                    movimiento.SucursalOrigenNombre =
                        sucursalOrigen;

                    movimiento.UbicacionOrigenNombre =
                        ubicacionOrigen;
                }

                if (esSalida)
                {
                    movimiento.DestinoExternoNombre =
                        Normalizar(dto.DestinoExternoNombre);

                    movimiento.DestinoExternoDireccion =
                        Normalizar(dto.DestinoExternoDireccion);

                    movimiento.ProveedorDestinoId =
                        proveedorDestino?.Id;

                    // La ubicación interna del activo se conserva.
                }
                else
                {
                    movimiento.SucursalDestinoId =
                        sucursalDestino!.Id;

                    movimiento.UbicacionDestinoId =
                        ubicacionDestino?.Id;

                    movimiento.SucursalDestinoNombre =
                        sucursalDestino.Nombre;

                    movimiento.UbicacionDestinoNombre =
                        ubicacionDestino?.Nombre;

                    // La ubicación interna se actualiza.
                    activo.SucursalId = sucursalDestino.Id;
                    activo.UbicacionId = ubicacionDestino?.Id;
                }

                activo.EstadoActivoId = estadoDestinoId;
                activo.FechaActualizacion = ahora;
                activo.UsuarioModificacion = usuarioId;

                context.ActivoMovimientos.Add(movimiento);

                await context.SaveChangesAsync(ct);

                await transaction.CommitAsync(ct);

                var detalle = await GetByIdAsync(
                    movimiento.Id,
                    empresaId,
                    ct);

                return ServiceResult<ActivoMovimientoDto>
                    .Ok(detalle!);
            }
            catch (Exception ex) when (EsConflictoConcurrencia(ex))
            {
                return ServiceResult<ActivoMovimientoDto>
                    .Fail(
                        "Otro usuario está modificando este activo. " +
                        "Intenta nuevamente.");
            }
        }

        // ==================================================
        // HELPERS
        // ==================================================

        private static string? Normalizar(string? texto)
        {
            return string.IsNullOrWhiteSpace(texto)
                ? null
                : texto.Trim();
        }

        private static bool EsConflictoConcurrencia(Exception ex)
        {
            if (ex is DbUpdateConcurrencyException)
                return true;

            if (ex is SqlException sql)
                return sql.Number is 1205 or 1222;

            if (ex is DbUpdateException db &&
                db.InnerException is SqlException interno)
            {
                return interno.Number is 1205 or 1222;
            }

            return false;
        }
    }
}
