using Microsoft.EntityFrameworkCore;
using Nextag.Api.Constants;
using Nextag.Api.Data;
using Nextag.Api.Models.Dto.Contratos;
using Nextag.Api.Models.Dto;
using Nextag.Api.Services.Contratos.Interface;
using Nextag.Api.Models.Domain;
using Microsoft.Data.SqlClient;

namespace Nextag.Api.Services.Contratos.Implementation
{
    public class ContratoService : IContratoService
    {
        private readonly NextagContext context;

        private const int DiasProximosAVencerDefault = 30;

        private const string MensajeNoEncontrado =
            "Contrato no encontrado.";

        private static readonly HashSet<string> PeriodicidadesValidas =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "UNICO",
                "SEMANAL",
                "QUINCENAL",
                "MENSUAL",
                "BIMESTRAL",
                "TRIMESTRAL",
                "SEMESTRAL",
                "ANUAL",
                "OTRO"
            };

        private static readonly TimeZoneInfo ZonaHoraria =
            TimeZoneInfo.FindSystemTimeZoneById(
                "Central Standard Time (Mexico)");

        public ContratoService(NextagContext context)
        {
            this.context = context;
        }

        private static DateTime ObtenerHoy()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                ZonaHoraria).Date;
        }

        public async Task<ServiceResult<ContratoListadoDto>> GetAllAsync(
    Guid empresaId,
    ContratoFiltroDto filtro,
    CancellationToken ct = default)
        {
            if (filtro.FechaInicioDesde.HasValue &&
                filtro.FechaInicioHasta.HasValue &&
                filtro.FechaInicioDesde.Value.Date >
                filtro.FechaInicioHasta.Value.Date)
            {
                return ServiceResult<ContratoListadoDto>.Fail(
                    "El rango de fechas de inicio no es válido.");
            }

            if (filtro.FechaFinDesde.HasValue &&
                filtro.FechaFinHasta.HasValue &&
                filtro.FechaFinDesde.Value.Date >
                filtro.FechaFinHasta.Value.Date)
            {
                return ServiceResult<ContratoListadoDto>.Fail(
                    "El rango de fechas de fin no es válido.");
            }

            var hoy = ObtenerHoy();

            var fechaLimite = hoy.AddDays(
                Math.Clamp(filtro.DiasProximosAVencer, 1, 365));

            var query = context.Contratos
                .AsNoTracking()
                .Where(x => x.EmpresaId == empresaId);

            if (filtro.Activo.HasValue)
            {
                if (filtro.Activo.Value)
                    query = query.Where(x => x.Activo == true);
                else
                    query = query.Where(x => x.Activo != true);
            }

            var buscar = filtro.Buscar?.Trim();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                query = query.Where(x =>
                    x.NumeroContrato.Contains(buscar) ||
                    x.Nombre.Contains(buscar) ||
                    (x.Descripcion != null &&
                        x.Descripcion.Contains(buscar)) ||
                    (x.Cliente != null &&
                        x.Cliente.Nombre.Contains(buscar)) ||
                    (x.Proveedor != null &&
                        x.Proveedor.Nombre.Contains(buscar)) ||
                    x.TipoContrato.Nombre.Contains(buscar));
            }

            if (filtro.TipoContratoId.HasValue)
            {
                query = query.Where(x =>
                    x.TipoContratoId == filtro.TipoContratoId.Value);
            }

            if (filtro.EstadoContratoId.HasValue)
            {
                query = query.Where(x =>
                    x.EstadoContratoId == filtro.EstadoContratoId.Value);
            }

            if (filtro.ClienteId.HasValue)
            {
                query = query.Where(x =>
                    x.ClienteId == filtro.ClienteId.Value);
            }

            if (filtro.ProveedorId.HasValue)
            {
                query = query.Where(x =>
                    x.ProveedorId == filtro.ProveedorId.Value);
            }

            if (filtro.RenovacionAutomatica.HasValue)
            {
                query = query.Where(x =>
                    x.RenovacionAutomatica ==
                    filtro.RenovacionAutomatica.Value);
            }

            var moneda = Normalizar(filtro.Moneda)?.ToUpperInvariant();

            if (moneda is not null)
            {
                query = query.Where(x => x.Moneda == moneda);
            }

            if (filtro.FechaInicioDesde.HasValue)
            {
                var fecha = filtro.FechaInicioDesde.Value.Date;

                query = query.Where(x =>
                    x.FechaInicio >= fecha);
            }

            if (filtro.FechaInicioHasta.HasValue)
            {
                var fecha = filtro.FechaInicioHasta.Value.Date;

                query = query.Where(x =>
                    x.FechaInicio <= fecha);
            }

            if (filtro.FechaFinDesde.HasValue)
            {
                var fecha = filtro.FechaFinDesde.Value.Date;

                query = query.Where(x =>
                    x.FechaFin.HasValue &&
                    x.FechaFin.Value >= fecha);
            }

            if (filtro.FechaFinHasta.HasValue)
            {
                var fecha = filtro.FechaFinHasta.Value.Date;

                query = query.Where(x =>
                    x.FechaFin.HasValue &&
                    x.FechaFin.Value <= fecha);
            }

            if (filtro.SinFechaFin.HasValue)
            {
                if (filtro.SinFechaFin.Value)
                    query = query.Where(x => x.FechaFin == null);
                else
                    query = query.Where(x => x.FechaFin != null);
            }

            if (filtro.Vencidos.HasValue)
            {
                if (filtro.Vencidos.Value)
                {
                    query = query.Where(x =>
                        x.FechaFin.HasValue &&
                        x.FechaFin.Value < hoy &&
                        (
                            x.EstadoContratoId == EstadosContratoEnum.Vigente ||
                            x.EstadoContratoId == EstadosContratoEnum.Suspendido ||
                            x.EstadoContratoId == EstadosContratoEnum.Vencido
                        ));
                }
                else
                {
                    query = query.Where(x =>
                        !(
                            x.FechaFin.HasValue &&
                            x.FechaFin.Value < hoy &&
                            (
                                x.EstadoContratoId == EstadosContratoEnum.Vigente ||
                                x.EstadoContratoId == EstadosContratoEnum.Suspendido ||
                                x.EstadoContratoId == EstadosContratoEnum.Vencido
                            )
                        ));
                }
            }

            if (filtro.ProximosAVencer.HasValue)
            {
                if (filtro.ProximosAVencer.Value)
                {
                    query = query.Where(x =>
                        x.FechaFin.HasValue &&
                        x.FechaFin.Value >= hoy &&
                        x.FechaFin.Value <= fechaLimite &&
                        (
                            x.EstadoContratoId == EstadosContratoEnum.Vigente ||
                            x.EstadoContratoId == EstadosContratoEnum.Suspendido
                        ));
                }
                else
                {
                    query = query.Where(x =>
                        !(
                            x.FechaFin.HasValue &&
                            x.FechaFin.Value >= hoy &&
                            x.FechaFin.Value <= fechaLimite &&
                            (
                                x.EstadoContratoId == EstadosContratoEnum.Vigente ||
                                x.EstadoContratoId == EstadosContratoEnum.Suspendido
                            )
                        ));
                }
            }

            var totalRegistros = await query.CountAsync(ct);

            var pagina = Math.Max(filtro.Pagina, 1);

            var registrosPorPagina = Math.Clamp(
                filtro.RegistrosPorPagina,
                1,
                200);

            var totalPaginas = (int)Math.Ceiling(
                totalRegistros / (double)registrosPorPagina);

            var desplazamiento = (long)(pagina - 1) *
                registrosPorPagina;

            if (desplazamiento > int.MaxValue ||
                desplazamiento >= totalRegistros)
            {
                return ServiceResult<ContratoListadoDto>.Ok(
                    new ContratoListadoDto
                    {
                        Items = new List<ContratoListDto>(),
                        TotalRegistros = totalRegistros,
                        Pagina = pagina,
                        RegistrosPorPagina = registrosPorPagina,
                        TotalPaginas = totalPaginas
                    });
            }

            var items = await query
                .OrderByDescending(x => x.FechaCreacion)
                .ThenByDescending(x => x.Id)
                .Skip((int)desplazamiento)
                .Take(registrosPorPagina)
                .Select(x => new ContratoListDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,

                    NumeroContrato = x.NumeroContrato,
                    Nombre = x.Nombre,

                    TipoContratoId = x.TipoContratoId,
                    TipoContratoNombre = x.TipoContrato.Nombre,

                    EstadoContratoId = x.EstadoContratoId,
                    EstadoContratoClave = x.EstadoContrato.Clave,
                    EstadoContratoNombre = x.EstadoContrato.Nombre,

                    ClienteId = x.ClienteId,
                    ClienteNombre = x.Cliente != null
                        ? x.Cliente.Nombre
                        : null,

                    ProveedorId = x.ProveedorId,
                    ProveedorNombre = x.Proveedor != null
                        ? x.Proveedor.Nombre
                        : null,

                    FechaInicio = x.FechaInicio,
                    FechaFin = x.FechaFin,

                    RenovacionAutomatica = x.RenovacionAutomatica,

                    Monto = x.Monto,
                    Moneda = x.Moneda,
                    PeriodicidadPago = x.PeriodicidadPago,

                    Activo = x.Activo == true,

                    TotalActivosAsociados = x.ContratoActivos.Count(a =>
                        a.FechaExclusion == null)

                })
                .ToListAsync(ct);

            foreach (var item in items)
            {
                CalcularVigencia(
                    item,
                    hoy,
                    filtro.DiasProximosAVencer);
            }

            return ServiceResult<ContratoListadoDto>.Ok(
                new ContratoListadoDto
                {
                    Items = items,
                    TotalRegistros = totalRegistros,
                    Pagina = pagina,
                    RegistrosPorPagina = registrosPorPagina,
                    TotalPaginas = totalPaginas
                });
        }

        public async Task<ContratoDetalleDto?> GetByIdAsync(
    Guid id,
    Guid empresaId,
    CancellationToken ct = default)
        {
            var detalle = await context.Contratos
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId)
                .Select(x => new ContratoDetalleDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,

                    NumeroContrato = x.NumeroContrato,
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion,

                    TipoContratoId = x.TipoContratoId,
                    TipoContratoNombre = x.TipoContrato.Nombre,

                    EstadoContratoId = x.EstadoContratoId,
                    EstadoContratoClave = x.EstadoContrato.Clave,
                    EstadoContratoNombre = x.EstadoContrato.Nombre,

                    ClienteId = x.ClienteId,
                    ClienteNombre = x.Cliente != null
                        ? x.Cliente.Nombre
                        : null,

                    ProveedorId = x.ProveedorId,
                    ProveedorNombre = x.Proveedor != null
                        ? x.Proveedor.Nombre
                        : null,

                    FechaInicio = x.FechaInicio,
                    FechaFin = x.FechaFin,
                    FechaFirma = x.FechaFirma,

                    RenovacionAutomatica = x.RenovacionAutomatica,
                    DiasAvisoNoRenovacion = x.DiasAvisoNoRenovacion,

                    Monto = x.Monto,
                    Moneda = x.Moneda,
                    PeriodicidadPago = x.PeriodicidadPago,
                    DiaPago = x.DiaPago,

                    IncluyeImpuestos = x.IncluyeImpuestos,

                    IncluyePreventivo = x.IncluyePreventivo,
                    IncluyeCorrectivo = x.IncluyeCorrectivo,
                    IncluyeRefacciones = x.IncluyeRefacciones,
                    IncluyeManoObra = x.IncluyeManoObra,

                    TiempoRespuestaHoras = x.TiempoRespuestaHoras,
                    HorarioAtencion = x.HorarioAtencion,
                    ServiciosIncluidos = x.ServiciosIncluidos,
                    CondicionesServicio = x.CondicionesServicio,

                    FechaCancelacion = x.FechaCancelacion,
                    MotivoCancelacion = x.MotivoCancelacion,
                    UsuarioCancelacion = x.UsuarioCancelacion,
                    FechaFinalizacion = x.FechaFinalizacion,

                    Observaciones = x.Observaciones,

                    Activo = x.Activo == true,

                    TotalActivosAsociados = x.ContratoActivos.Count(a =>
                        a.FechaExclusion == null),

                    TotalRenovaciones = x.ContratoRenovacions.Count(),

                    FechaCreacion = x.FechaCreacion,
                    UsuarioCreacion = x.UsuarioCreacion,
                    FechaActualizacion = x.FechaActualizacion,
                    UsuarioModificacion = x.UsuarioModificacion
                })
                .FirstOrDefaultAsync(ct);

            if (detalle is not null)
            {
                CalcularVigencia(
                    detalle,
                    ObtenerHoy(),
                    DiasProximosAVencerDefault);
            }

            return detalle;
        }

        public async Task<ServiceResult<ContratoDetalleDto>> CreateAsync(
    ContratoGuardarDto dto,
    Guid empresaId,
    Guid usuarioId,
    CancellationToken ct = default)
        {
            var error = await ValidarDatosAsync(
                dto,
                empresaId,
                null,
                ct);

            if (error is not null)
            {
                return ServiceResult<ContratoDetalleDto>.Fail(error);
            }

            var estadoDisponible = await EstadoDisponibleAsync(
                EstadosContratoEnum.Borrador,
                ct);

            if (!estadoDisponible)
            {
                return ServiceResult<ContratoDetalleDto>.Fail(
                    "El estado Borrador no se encuentra disponible.");
            }

            var contrato = new Contrato
            {
                EmpresaId = empresaId,
                EstadoContratoId = EstadosContratoEnum.Borrador,
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                UsuarioCreacion = usuarioId
            };

            MapearCampos(contrato, dto);

            try
            {
                context.Contratos.Add(contrato);

                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (EsDuplicado(ex))
            {
                return ServiceResult<ContratoDetalleDto>.Fail(
                    "Ya existe un contrato con ese número en la empresa.");
            }

            var detalle = await GetByIdAsync(
                contrato.Id,
                empresaId,
                ct);

            return ServiceResult<ContratoDetalleDto>.Ok(detalle!);
        }

        public async Task<ServiceResult<ContratoDetalleDto>> UpdateAsync(
    Guid id,
    ContratoGuardarDto dto,
    Guid empresaId,
    Guid usuarioId,
    CancellationToken ct = default)
        {
            var contrato = await ObtenerContratoAsync(
                id,
                empresaId,
                ct);

            if (contrato is null)
            {
                return ServiceResult<ContratoDetalleDto>.Fail(
                    MensajeNoEncontrado);
            }

            if (contrato.Activo != true)
            {
                return ServiceResult<ContratoDetalleDto>.Fail(
                    "No se puede modificar un contrato dado de baja.");
            }

            if (contrato.EstadoContratoId == EstadosContratoEnum.Cancelado ||
                contrato.EstadoContratoId == EstadosContratoEnum.Finalizado)
            {
                return ServiceResult<ContratoDetalleDto>.Fail(
                    "No se puede modificar un contrato cancelado o finalizado.");
            }

            var error = await ValidarDatosAsync(
                dto,
                empresaId,
                contrato,
                ct);

            if (error is not null)
            {
                return ServiceResult<ContratoDetalleDto>.Fail(error);
            }

            // Una vez formalizado, la vigencia no se modifica
            // desde la edición general.
            // Los cambios de vigencia se administrarán en el bloque 9H.

            if (contrato.EstadoContratoId != EstadosContratoEnum.Borrador)
            {
                if (contrato.FechaInicio.Date !=
                        dto.FechaInicio!.Value.Date ||
                    contrato.FechaFin?.Date != dto.FechaFin?.Date)
                {
                    return ServiceResult<ContratoDetalleDto>.Fail(
                        "La vigencia de un contrato formalizado debe modificarse mediante una renovación.");
                }
            }

            MapearCampos(contrato, dto);

            MarcarActualizacion(
                contrato,
                usuarioId);

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (EsDuplicado(ex))
            {
                return ServiceResult<ContratoDetalleDto>.Fail(
                    "Ya existe otro contrato con ese número en la empresa.");
            }

            var detalle = await GetByIdAsync(
                contrato.Id,
                empresaId,
                ct);

            return ServiceResult<ContratoDetalleDto>.Ok(detalle!);
        }

        public Task<ServiceResult<ContratoDetalleDto>> ActivarAsync(
    Guid id,
    Guid empresaId,
    Guid usuarioId,
    CancellationToken ct = default)
        {
            return CambiarEstadoAsync(
                id,
                empresaId,
                usuarioId,
                EstadosContratoEnum.Vigente,
                new[] { EstadosContratoEnum.Borrador },
                validarVigencia: true,
                ct: ct);
        }

        public Task<ServiceResult<ContratoDetalleDto>> SuspenderAsync(
    Guid id,
    Guid empresaId,
    Guid usuarioId,
    CancellationToken ct = default)
        {
            return CambiarEstadoAsync(
                id,
                empresaId,
                usuarioId,
                EstadosContratoEnum.Suspendido,
                new[] { EstadosContratoEnum.Vigente },
                validarVigencia: false,
                ct: ct);
        }

        public Task<ServiceResult<ContratoDetalleDto>> ReanudarAsync(
    Guid id,
    Guid empresaId,
    Guid usuarioId,
    CancellationToken ct = default)
        {
            return CambiarEstadoAsync(
                id,
                empresaId,
                usuarioId,
                EstadosContratoEnum.Vigente,
                new[] { EstadosContratoEnum.Suspendido },
                validarVigencia: true,
                ct: ct);
        }

        public Task<ServiceResult<ContratoDetalleDto>> CancelarAsync(
    Guid id,
    ContratoCancelarDto dto,
    Guid empresaId,
    Guid usuarioId,
    CancellationToken ct = default)
        {
            var motivo = Normalizar(dto.Motivo);

            if (motivo is null)
            {
                return Task.FromResult(
                    ServiceResult<ContratoDetalleDto>.Fail(
                        "El motivo de cancelación es obligatorio."));
            }

            if (motivo.Length > 500)
            {
                return Task.FromResult(
                    ServiceResult<ContratoDetalleDto>.Fail(
                        "El motivo de cancelación no puede exceder 500 caracteres."));
            }

            return CambiarEstadoAsync(
                id,
                empresaId,
                usuarioId,
                EstadosContratoEnum.Cancelado,
                new[]
                {
            EstadosContratoEnum.Borrador,
            EstadosContratoEnum.Vigente,
            EstadosContratoEnum.Suspendido
                },
                validarVigencia: false,
                motivoCancelacion: motivo,
                ct: ct);
        }

        public Task<ServiceResult<ContratoDetalleDto>> FinalizarAsync(
    Guid id,
    Guid empresaId,
    Guid usuarioId,
    CancellationToken ct = default)
        {
            return CambiarEstadoAsync(
                id,
                empresaId,
                usuarioId,
                EstadosContratoEnum.Finalizado,
                new[]
                {
            EstadosContratoEnum.Vigente,
            EstadosContratoEnum.Suspendido,
            EstadosContratoEnum.Vencido
                },
                validarVigencia: false,
                ct: ct);
        }

        private async Task<ServiceResult<ContratoDetalleDto>> CambiarEstadoAsync(
    Guid id,
    Guid empresaId,
    Guid usuarioId,
    int estadoDestino,
    int[] estadosPermitidos,
    bool validarVigencia,
    string? motivoCancelacion = null,
    CancellationToken ct = default)
        {
            var contrato = await ObtenerContratoAsync(
                id,
                empresaId,
                ct);

            if (contrato is null)
            {
                return ServiceResult<ContratoDetalleDto>.Fail(
                    MensajeNoEncontrado);
            }

            if (contrato.Activo != true)
            {
                return ServiceResult<ContratoDetalleDto>.Fail(
                    "El contrato se encuentra dado de baja.");
            }

            // Si ya se encuentra en el estado solicitado,
            // devolver su información sin modificar la auditoría.

            if (contrato.EstadoContratoId == estadoDestino)
            {
                var actual = await GetByIdAsync(
                    id,
                    empresaId,
                    ct);

                return ServiceResult<ContratoDetalleDto>.Ok(actual!);
            }

            var hoy = ObtenerHoy();

            var estadoActual = contrato.EstadoContratoId;

            // Mientras no exista el proceso automático de vencimientos
            // del bloque 9M, considerar vencidos los contratos cuya
            // fecha de fin ya terminó.

            if (
                contrato.FechaFin.HasValue &&
                contrato.FechaFin.Value.Date < hoy &&
                (
                    estadoActual == EstadosContratoEnum.Vigente ||
                    estadoActual == EstadosContratoEnum.Suspendido
                ))
            {
                estadoActual = EstadosContratoEnum.Vencido;
            }

            if (!estadosPermitidos.Contains(estadoActual))
            {
                return ServiceResult<ContratoDetalleDto>.Fail(
                    "La operación no está permitida para el estado actual del contrato.");
            }

            var estadoDisponible = await EstadoDisponibleAsync(
                estadoDestino,
                ct);

            if (!estadoDisponible)
            {
                return ServiceResult<ContratoDetalleDto>.Fail(
                    "El estado de destino no está disponible.");
            }

            if (validarVigencia)
            {
                if (contrato.FechaInicio.Date > hoy)
                {
                    return ServiceResult<ContratoDetalleDto>.Fail(
                        "El contrato todavía no ha iniciado su vigencia.");
                }

                if (contrato.FechaFin.HasValue &&
                    contrato.FechaFin.Value.Date < hoy)
                {
                    return ServiceResult<ContratoDetalleDto>.Fail(
                        "No se puede activar o reanudar un contrato vencido.");
                }
            }

            // Verificar catálogos activos al formalizar por primera vez.

            if (
                contrato.EstadoContratoId == EstadosContratoEnum.Borrador &&
                estadoDestino == EstadosContratoEnum.Vigente)
            {
                var error = await ValidarRelacionesAsync(
                    empresaId,
                    contrato.TipoContratoId,
                    contrato.ClienteId,
                    contrato.ProveedorId,
                    null,
                    ct);

                if (error is not null)
                {
                    return ServiceResult<ContratoDetalleDto>.Fail(error);
                }
            }

            contrato.EstadoContratoId = estadoDestino;

            if (estadoDestino == EstadosContratoEnum.Cancelado)
            {
                contrato.FechaCancelacion = DateTime.UtcNow;
                contrato.UsuarioCancelacion = usuarioId;
                contrato.MotivoCancelacion = motivoCancelacion;
            }

            if (estadoDestino == EstadosContratoEnum.Finalizado)
            {
                contrato.FechaFinalizacion = DateTime.UtcNow;
            }

            MarcarActualizacion(
                contrato,
                usuarioId);

            await context.SaveChangesAsync(ct);

            var detalle = await GetByIdAsync(
                id,
                empresaId,
                ct);

            return ServiceResult<ContratoDetalleDto>.Ok(detalle!);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(
    Guid id,
    Guid empresaId,
    Guid usuarioId,
    CancellationToken ct = default)
        {
            var contrato = await ObtenerContratoAsync(
                id,
                empresaId,
                ct);

            if (contrato is null)
            {
                return ServiceResult<bool>.Fail(
                    MensajeNoEncontrado);
            }

            if (contrato.Activo != true)
            {
                return ServiceResult<bool>.Ok(true);
            }

            if (contrato.EstadoContratoId != EstadosContratoEnum.Borrador)
            {
                return ServiceResult<bool>.Fail(
                    "Únicamente se pueden eliminar contratos en estado Borrador.");
            }

            var tieneActivos = await context.ContratoActivos
                .AnyAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.ContratoId == id &&
                    x.FechaExclusion == null,
                    ct);

            if (tieneActivos)
            {
                return ServiceResult<bool>.Fail(
                    "No se puede eliminar un contrato que tiene activos asociados.");
            }

            var tieneRenovaciones = await context.ContratoRenovacions
                .AnyAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.ContratoId == id,
                    ct);

            if (tieneRenovaciones)
            {
                return ServiceResult<bool>.Fail(
                    "No se puede eliminar un contrato con historial de renovaciones.");
            }

            contrato.Activo = false;

            MarcarActualizacion(
                contrato,
                usuarioId);

            await context.SaveChangesAsync(ct);

            return ServiceResult<bool>.Ok(true);
        }

        private async Task<string?> ValidarDatosAsync(
    ContratoGuardarDto dto,
    Guid empresaId,
    Contrato? contratoActual,
    CancellationToken ct)
        {
            var numeroContrato = dto.NumeroContrato?
                .Trim()
                .ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(numeroContrato))
                return "El número de contrato es obligatorio.";

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return "El nombre del contrato es obligatorio.";

            if (dto.TipoContratoId == Guid.Empty)
                return "Selecciona un tipo de contrato.";

            if (!dto.FechaInicio.HasValue ||
                dto.FechaInicio.Value == DateTime.MinValue)
            {
                return "La fecha de inicio es obligatoria.";
            }

            if (dto.FechaFin.HasValue &&
                dto.FechaFin.Value.Date <
                dto.FechaInicio.Value.Date)
            {
                return "La fecha de fin no puede ser anterior a la fecha de inicio.";
            }

            if (dto.ClienteId == Guid.Empty)
                return "El cliente seleccionado no es válido.";

            if (dto.ProveedorId == Guid.Empty)
                return "El proveedor seleccionado no es válido.";

            if (dto.Monto.HasValue)
            {
                if (dto.Monto.Value < 0 ||
                    dto.Monto.Value > 9999999999999999.99m)
                {
                    return "El monto del contrato no es válido.";
                }

                if (decimal.Round(dto.Monto.Value, 2) != dto.Monto.Value)
                {
                    return "El monto admite un máximo de dos decimales.";
                }
            }

            var moneda = Normalizar(dto.Moneda)?
                .ToUpperInvariant();

            if (moneda is null ||
                moneda.Length != 3 ||
                moneda.Any(x => x < 'A' || x > 'Z'))
            {
                return "La moneda debe contener un código válido de tres letras.";
            }

            var periodicidad = Normalizar(dto.PeriodicidadPago)?
                .ToUpperInvariant();

            if (periodicidad is not null &&
                !PeriodicidadesValidas.Contains(periodicidad))
            {
                return "La periodicidad de pago no es válida.";
            }

            if (dto.DiaPago.HasValue &&
                (dto.DiaPago.Value < 1 ||
                 dto.DiaPago.Value > 31))
            {
                return "El día de pago debe estar entre 1 y 31.";
            }

            if (dto.TiempoRespuestaHoras.HasValue)
            {
                if (dto.TiempoRespuestaHoras.Value < 0 ||
                    dto.TiempoRespuestaHoras.Value > 99999999.99m)
                {
                    return "El tiempo de respuesta no es válido.";
                }

                if (decimal.Round(dto.TiempoRespuestaHoras.Value, 2) !=
                    dto.TiempoRespuestaHoras.Value)
                {
                    return "El tiempo de respuesta admite un máximo de dos decimales.";
                }
            }

            if (dto.ServiciosIncluidos.HasValue &&
                dto.ServiciosIncluidos.Value < 0)
            {
                return "Los servicios incluidos no pueden ser negativos.";
            }

            if (dto.DiasAvisoNoRenovacion.HasValue &&
                dto.DiasAvisoNoRenovacion.Value < 0)
            {
                return "Los días de aviso no pueden ser negativos.";
            }

            if (dto.RenovacionAutomatica &&
                !dto.FechaFin.HasValue)
            {
                return "Un contrato con renovación automática debe tener fecha de fin.";
            }

            if (dto.DiasAvisoNoRenovacion.HasValue &&
                !dto.FechaFin.HasValue)
            {
                return "Se requiere una fecha de fin para configurar el aviso de no renovación.";
            }

            var excluirId = contratoActual?.Id;

            var existeNumero = await context.Contratos
                .AnyAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.NumeroContrato == numeroContrato &&
                    (
                        !excluirId.HasValue ||
                        x.Id != excluirId.Value
                    ),
                    ct);

            if (existeNumero)
            {
                return "Ya existe un contrato con ese número en la empresa.";
            }

            return await ValidarRelacionesAsync(
                empresaId,
                dto.TipoContratoId,
                dto.ClienteId,
                dto.ProveedorId,
                contratoActual,
                ct);
        }

        private async Task<string?> ValidarRelacionesAsync(
    Guid empresaId,
    Guid tipoContratoId,
    Guid? clienteId,
    Guid? proveedorId,
    Contrato? contratoActual,
    CancellationToken ct)
        {
            var tipoContrato = await context.TipoContratos
                .AsNoTracking()
                .Where(x =>
                    x.Id == tipoContratoId &&
                    x.EmpresaId == empresaId)
                .Select(x => new
                {
                    x.Id,
                    x.Activo
                })
                .FirstOrDefaultAsync(ct);

            if (tipoContrato is null)
            {
                return "El tipo de contrato seleccionado no pertenece a la empresa o no existe.";
            }

            var conservaTipo =
                contratoActual is not null &&
                contratoActual.TipoContratoId == tipoContratoId;

            if (!conservaTipo &&
                tipoContrato.Activo != true)
            {
                return "El tipo de contrato seleccionado se encuentra inactivo.";
            }

            if (clienteId.HasValue)
            {
                var cliente = await context.Clientes
                    .AsNoTracking()
                    .Where(x =>
                        x.Id == clienteId.Value &&
                        x.EmpresaId == empresaId)
                    .Select(x => new
                    {
                        x.Id,
                        x.Activo
                    })
                    .FirstOrDefaultAsync(ct);

                if (cliente is null)
                {
                    return "El cliente seleccionado no pertenece a la empresa o no existe.";
                }

                var conservaCliente =
                    contratoActual is not null &&
                    contratoActual.ClienteId == clienteId;

                if (!conservaCliente &&
                    cliente.Activo != true)
                {
                    return "El cliente seleccionado se encuentra inactivo.";
                }
            }

            if (proveedorId.HasValue)
            {
                var proveedor = await context.Proveedors
                    .AsNoTracking()
                    .Where(x =>
                        x.Id == proveedorId.Value &&
                        x.EmpresaId == empresaId)
                    .Select(x => new
                    {
                        x.Id,
                        x.Activo
                    })
                    .FirstOrDefaultAsync(ct);

                if (proveedor is null)
                {
                    return "El proveedor seleccionado no pertenece a la empresa o no existe.";
                }

                var conservaProveedor =
                    contratoActual is not null &&
                    contratoActual.ProveedorId == proveedorId;

                if (!conservaProveedor &&
                    proveedor.Activo != true)
                {
                    return "El proveedor seleccionado se encuentra inactivo.";
                }
            }

            return null;
        }

        private Task<bool> EstadoDisponibleAsync(
    int estadoId,
    CancellationToken ct)
        {
            return context.EstadoContratos
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == estadoId &&
                    x.Activo == true,
                    ct);
        }



        private Task<Contrato?> ObtenerContratoAsync(
    Guid id,
    Guid empresaId,
    CancellationToken ct)
        {
            return context.Contratos
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId,
                    ct);
        }

        private static void MapearCampos(
    Contrato contrato,
    ContratoGuardarDto dto)
        {
            contrato.NumeroContrato = dto.NumeroContrato
                .Trim()
                .ToUpperInvariant();

            contrato.Nombre = dto.Nombre.Trim();

            contrato.Descripcion = Normalizar(
                dto.Descripcion);

            contrato.TipoContratoId = dto.TipoContratoId;

            contrato.ClienteId = dto.ClienteId;

            contrato.ProveedorId = dto.ProveedorId;

            contrato.FechaInicio = dto.FechaInicio!.Value.Date;

            contrato.FechaFin = dto.FechaFin?.Date;

            contrato.FechaFirma = dto.FechaFirma?.Date;

            contrato.RenovacionAutomatica =
                dto.RenovacionAutomatica;

            contrato.DiasAvisoNoRenovacion =
                dto.DiasAvisoNoRenovacion;

            contrato.Monto = dto.Monto;

            contrato.Moneda = dto.Moneda
                .Trim()
                .ToUpperInvariant();

            contrato.PeriodicidadPago = Normalizar(
                dto.PeriodicidadPago)?.ToUpperInvariant();

            contrato.DiaPago = dto.DiaPago;

            contrato.IncluyeImpuestos = dto.IncluyeImpuestos;

            contrato.IncluyePreventivo = dto.IncluyePreventivo;

            contrato.IncluyeCorrectivo = dto.IncluyeCorrectivo;

            contrato.IncluyeRefacciones = dto.IncluyeRefacciones;

            contrato.IncluyeManoObra = dto.IncluyeManoObra;

            contrato.TiempoRespuestaHoras =
                dto.TiempoRespuestaHoras;

            contrato.HorarioAtencion = Normalizar(
                dto.HorarioAtencion);

            contrato.ServiciosIncluidos =
                dto.ServiciosIncluidos;

            contrato.CondicionesServicio = Normalizar(
                dto.CondicionesServicio);

            contrato.Observaciones = Normalizar(
                dto.Observaciones);
        }

        private static void CalcularVigencia(
    ContratoListDto contrato,
    DateTime hoy,
    int diasProximosAVencer)
        {
            contrato.DiasRestantes = null;

            contrato.EstaVencido = false;

            contrato.ProximoAVencer = false;

            contrato.PendienteInicio = false;

            if (!contrato.Activo)
                return;

            var estadoOperativo =
                contrato.EstadoContratoId == EstadosContratoEnum.Vigente ||
                contrato.EstadoContratoId == EstadosContratoEnum.Suspendido;

            contrato.PendienteInicio =
                (
                    estadoOperativo ||
                    contrato.EstadoContratoId == EstadosContratoEnum.Borrador
                ) &&
                contrato.FechaInicio.Date > hoy;

            if (!contrato.FechaFin.HasValue)
                return;

            contrato.DiasRestantes =
                (contrato.FechaFin.Value.Date - hoy.Date).Days;

            contrato.EstaVencido =
                contrato.DiasRestantes.Value < 0 &&
                (
                    estadoOperativo ||
                    contrato.EstadoContratoId == EstadosContratoEnum.Vencido
                );

            contrato.ProximoAVencer =
                estadoOperativo &&
                contrato.DiasRestantes.Value >= 0 &&
                contrato.DiasRestantes.Value <= diasProximosAVencer;
        }

        private static void MarcarActualizacion(
    Contrato contrato,
    Guid usuarioId)
        {
            contrato.FechaActualizacion = DateTime.UtcNow;

            contrato.UsuarioModificacion = usuarioId;
        }

        private static string? Normalizar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? null
                : valor.Trim();
        }


        private static bool EsDuplicado(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sql &&
                (sql.Number == 2601 || sql.Number == 2627);
        }

    }
}
