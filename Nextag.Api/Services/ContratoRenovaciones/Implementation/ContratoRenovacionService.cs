using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Constants;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto;
using Nextag.Api.Models.Dto.ContratoRenovaciones;
using Nextag.Api.Services.ContratoRenovaciones.Interface;
using Nextag.Api.Services.Contratos.Interface;
using System.Data;
using System.Linq.Expressions;

namespace Nextag.Api.Services.ContratoRenovaciones.Implementation
{
    public class ContratoRenovacionService
        : IContratoRenovacionService
    {
        private readonly NextagContext context;

        private readonly IContratoService contratoService;

        private const string ContratoNoEncontrado =
            "Contrato no encontrado.";

        private const string RenovacionNoEncontrada =
            "Renovación no encontrada.";

        private const string MensajeConflicto =
            "La información del contrato cambió durante la operación. Vuelve a consultar.";

        private static readonly TimeZoneInfo ZonaHoraria =
            TimeZoneInfo.FindSystemTimeZoneById(
                "Central Standard Time (Mexico)");

        public ContratoRenovacionService(
            NextagContext context,
            IContratoService contratoService)
        {
            this.context = context;

            this.contratoService = contratoService;
        }

        private static DateTime ObtenerHoy()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                ZonaHoraria).Date;
        }

        private static readonly Expression<
        Func<ContratoRenovacion, ContratoRenovacionDto>>
        Proyeccion =
        x => new ContratoRenovacionDto
        {
            Id = x.Id,
            EmpresaId = x.EmpresaId,
            ContratoId = x.ContratoId,
            NumeroRenovacion = x.NumeroRenovacion,
            FechaInicioAnterior = x.FechaInicioAnterior,
            FechaFinAnterior = x.FechaFinAnterior,
            FechaInicioNueva = x.FechaInicioNueva,
            FechaFinNueva = x.FechaFinNueva,
            MontoAnterior = x.MontoAnterior,
            MontoNuevo = x.MontoNuevo,
            Motivo = x.Motivo,
            Observaciones = x.Observaciones,
            FechaRenovacion = x.FechaRenovacion,
            FechaCreacion = x.FechaCreacion,
            UsuarioCreacion = x.UsuarioCreacion,
            FechaActualizacion = x.FechaActualizacion,
            UsuarioModificacion = x.UsuarioModificacion
        };

        public async Task<ServiceResult<List<ContratoRenovacionDto>>> GetAllAsync(
        Guid contratoId,
        Guid empresaId,
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
                return ServiceResult<List<ContratoRenovacionDto>>
                    .Fail(ContratoNoEncontrado);
            }

            var items = await context.ContratoRenovacions
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.ContratoId == contratoId)
                .OrderByDescending(x => x.NumeroRenovacion)
                .ThenByDescending(x => x.Id)
                .Select(Proyeccion)
                .ToListAsync(ct);

            return ServiceResult<List<ContratoRenovacionDto>>
                .Ok(items);
        }

        public async Task<ServiceResult<ContratoRenovacionDto>>
        GetByIdAsync(
            Guid contratoId,
            Guid id,
            Guid empresaId,
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
                return ServiceResult<ContratoRenovacionDto>
                    .Fail(ContratoNoEncontrado);
            }

            var detalle = await context.ContratoRenovacions
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId &&
                    x.ContratoId == contratoId)
                .Select(Proyeccion)
                .FirstOrDefaultAsync(ct);

            if (detalle is null)
            {
                return ServiceResult<ContratoRenovacionDto>
                    .Fail(RenovacionNoEncontrada);
            }

            return ServiceResult<ContratoRenovacionDto>
                .Ok(detalle);
        }

        public async Task<
    ServiceResult<ContratoRenovacionInformacionDto>>
    GetInformacionAsync(
        Guid contratoId,
        Guid empresaId,
        CancellationToken ct = default)
        {
            var contrato = await context.Contratos
                .AsNoTracking()
                .Include(x => x.EstadoContrato)
                .FirstOrDefaultAsync(x =>
                    x.Id == contratoId &&
                    x.EmpresaId == empresaId,
                    ct);

            if (contrato is null)
            {
                return ServiceResult<ContratoRenovacionInformacionDto>
                    .Fail(ContratoNoEncontrado);
            }

            var hoy = ObtenerHoy();

            var estadoEfectivo = ObtenerEstadoEfectivo(
                contrato,
                hoy);

            var error = ValidarContratoParaRenovar(
                contrato,
                hoy);

            var queryRenovaciones = context.ContratoRenovacions
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.ContratoId == contratoId);

            var totalRenovaciones =
                await queryRenovaciones.CountAsync(ct);

            var ultimaRenovacion =
                await queryRenovaciones
                    .Select(x => (int?)x.NumeroRenovacion)
                    .MaxAsync(ct) ?? 0;

            var totalActivos =
                await context.ContratoActivos
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.EmpresaId == empresaId &&
                        x.ContratoId == contratoId &&
                        x.FechaExclusion == null,
                        ct);

            if (ultimaRenovacion == int.MaxValue)
            {
                error ??=
                    "Se alcanzó el límite de consecutivos de renovación.";
            }

            var resultado = new ContratoRenovacionInformacionDto
            {
                ContratoId = contrato.Id,

                NumeroContrato = contrato.NumeroContrato,

                Nombre = contrato.Nombre,

                EstadoContratoId = contrato.EstadoContratoId,

                EstadoEfectivoId = estadoEfectivo,

                EstadoContratoNombre =
                    contrato.EstadoContrato.Nombre,

                FechaInicioActual = contrato.FechaInicio,

                FechaFinActual = contrato.FechaFin,

                MontoActual = contrato.Monto,

                Moneda = contrato.Moneda,

                RenovacionAutomatica =
                    contrato.RenovacionAutomatica,

                DiasAvisoNoRenovacion =
                    contrato.DiasAvisoNoRenovacion,

                TotalRenovaciones = totalRenovaciones,

                UltimaRenovacion = ultimaRenovacion,

                SiguienteRenovacion =
                    ultimaRenovacion == int.MaxValue
                        ? 0
                        : ultimaRenovacion + 1,

                TotalActivosAsociados = totalActivos,

                Activo = contrato.Activo == true,

                EstaVencido =
                    estadoEfectivo == EstadosContratoEnum.Vencido,

                PuedeRenovar = error is null,

                MotivoNoDisponible = error,

                FechaActualizacion = contrato.FechaActualizacion
            };

            if (resultado.PuedeRenovar)
            {
                if (estadoEfectivo == EstadosContratoEnum.Vigente)
                {
                    resultado.FechaInicioSugerida =
                        contrato.FechaInicio.Date;
                }
                else
                {
                    resultado.FechaInicioSugerida = hoy;
                }

                // Sugerir una extensión de un año.
                // El usuario podrá modificar esta fecha.

                var fechaBase =
                    estadoEfectivo == EstadosContratoEnum.Vencido
                        ? hoy
                        : contrato.FechaFin!.Value.Date;

                if (fechaBase.Year < 9999)
                {
                    resultado.FechaFinSugerida =
                        fechaBase.AddYears(1);
                }
            }

            return ServiceResult<ContratoRenovacionInformacionDto>
                .Ok(resultado);
        }

        public async Task<
    ServiceResult<ContratoRenovacionResultadoDto>>
    RenovarAsync(
        Guid contratoId,
        ContratoRenovacionGuardarDto dto,
        Guid empresaId,
        Guid usuarioId,
        CancellationToken ct = default)
        {
            var errorDatos = ValidarDatos(dto);

            if (errorDatos is not null)
            {
                return ServiceResult<ContratoRenovacionResultadoDto>
                    .Fail(errorDatos);
            }

            ContratoRenovacionResultadoDto? resultado = null;

            try
            {
                await using (
                    var tx = await context.Database
                        .BeginTransactionAsync(
                            IsolationLevel.Serializable,
                            ct))
                {
                    var contrato = await context.Contratos
                        .FirstOrDefaultAsync(x =>
                            x.Id == contratoId &&
                            x.EmpresaId == empresaId,
                            ct);

                    if (contrato is null)
                    {
                        return ServiceResult<ContratoRenovacionResultadoDto>
                            .Fail(ContratoNoEncontrado);
                    }

                    var hoy = ObtenerHoy();

                    var errorContrato =
                        ValidarContratoParaRenovar(
                            contrato,
                            hoy);

                    if (errorContrato is not null)
                    {
                        return ServiceResult<ContratoRenovacionResultadoDto>
                            .Fail(errorContrato);
                    }

                    var ultimaRenovacion =
                        await context.ContratoRenovacions
                            .AsNoTracking()
                            .Where(x =>
                                x.EmpresaId == empresaId &&
                                x.ContratoId == contratoId)
                            .Select(x =>
                                (int?)x.NumeroRenovacion)
                            .MaxAsync(ct) ?? 0;

                    // Verificar que el contrato no haya cambiado
                    // desde que se consultó la información
                    // para preparar la renovación.

                    if (
                        contrato.FechaInicio.Date !=
                            dto.FechaInicioEsperada!.Value.Date ||

                        contrato.FechaFin?.Date !=
                            dto.FechaFinEsperada?.Date ||

                        contrato.Monto != dto.MontoEsperado ||

                        ultimaRenovacion !=
                            dto.UltimaRenovacionEsperada!.Value ||

                        contrato.FechaActualizacion !=
                            dto.FechaActualizacionEsperada
                    )
                    {
                        return ServiceResult<ContratoRenovacionResultadoDto>
                            .Fail(MensajeConflicto);
                    }

                    var fechaInicioNueva =
                        dto.FechaInicioNueva!.Value.Date;

                    var fechaFinNueva =
                        dto.FechaFinNueva!.Value.Date;

                    var errorVigencia =
                        ValidarNuevaVigencia(
                            contrato,
                            fechaInicioNueva,
                            fechaFinNueva,
                            hoy);

                    if (errorVigencia is not null)
                    {
                        return ServiceResult<ContratoRenovacionResultadoDto>
                            .Fail(errorVigencia);
                    }

                    // Confirmar que el estado Vigente se encuentre
                    // disponible en el catálogo global.

                    var estadoVigenteDisponible =
                        await context.EstadoContratos
                            .AsNoTracking()
                            .AnyAsync(x =>
                                x.Id == EstadosContratoEnum.Vigente &&
                                x.Activo == true,
                                ct);

                    if (!estadoVigenteDisponible)
                    {
                        return ServiceResult<ContratoRenovacionResultadoDto>
                            .Fail(
                                "El estado Vigente no se encuentra disponible.");
                    }

                    if (ultimaRenovacion == int.MaxValue)
                    {
                        return ServiceResult<ContratoRenovacionResultadoDto>
                            .Fail(
                                "Se alcanzó el límite de consecutivos de renovación.");
                    }

                    var numeroRenovacion =
                        ultimaRenovacion + 1;

                    var montoNuevo =
                        dto.ConservarMontoActual
                            ? contrato.Monto
                            : dto.MontoNuevo;

                    var ahora = DateTime.UtcNow;

                    // Registrar el historial antes de modificar
                    // los valores del contrato principal.

                    var renovacion = new ContratoRenovacion
                    {
                        EmpresaId = empresaId,

                        ContratoId = contratoId,

                        NumeroRenovacion = numeroRenovacion,

                        FechaInicioAnterior =
                            contrato.FechaInicio,

                        FechaFinAnterior =
                            contrato.FechaFin,

                        FechaInicioNueva =
                            fechaInicioNueva,

                        FechaFinNueva =
                            fechaFinNueva,

                        MontoAnterior =
                            contrato.Monto,

                        MontoNuevo =
                            montoNuevo,

                        Motivo =
                            Normalizar(dto.Motivo),

                        Observaciones =
                            Normalizar(dto.Observaciones),

                        FechaRenovacion = ahora,

                        FechaCreacion = ahora,

                        UsuarioCreacion = usuarioId
                    };

                    context.ContratoRenovacions.Add(
                        renovacion);

                    // Actualizar el contrato principal.

                    contrato.FechaInicio =
                        fechaInicioNueva;

                    contrato.FechaFin =
                        fechaFinNueva;

                    contrato.Monto =
                        montoNuevo;

                    contrato.EstadoContratoId =
                        EstadosContratoEnum.Vigente;

                    contrato.FechaActualizacion =
                        ahora;

                    contrato.UsuarioModificacion =
                        usuarioId;

                    await context.SaveChangesAsync(ct);

                    // Consultar la información dentro de la misma
                    // transacción antes de confirmarla.

                    var renovacionDetalle =
                        await context.ContratoRenovacions
                            .AsNoTracking()
                            .Where(x =>
                                x.Id == renovacion.Id &&
                                x.EmpresaId == empresaId &&
                                x.ContratoId == contratoId)
                            .Select(Proyeccion)
                            .FirstAsync(ct);

                    var contratoDetalle =
                        await contratoService.GetByIdAsync(
                            contratoId,
                            empresaId,
                            ct);

                    if (contratoDetalle is null)
                    {
                        return ServiceResult<ContratoRenovacionResultadoDto>
                            .Fail(ContratoNoEncontrado);
                    }

                    resultado = new ContratoRenovacionResultadoDto
                    {
                        Renovacion = renovacionDetalle,

                        Contrato = contratoDetalle
                    };

                    await tx.CommitAsync(ct);
                }
            }
            catch (Exception ex) when (EsConflictoSql(ex))
            {
                return ServiceResult<ContratoRenovacionResultadoDto>
                    .Fail(MensajeConflicto);
            }

            return ServiceResult<ContratoRenovacionResultadoDto>
                .Ok(resultado!);
        }

        private static string? ValidarDatos(
    ContratoRenovacionGuardarDto dto)
        {
            if (!dto.FechaInicioNueva.HasValue ||
                dto.FechaInicioNueva.Value ==
                    DateTime.MinValue)
            {
                return "La nueva fecha de inicio es obligatoria.";
            }

            if (!dto.FechaFinNueva.HasValue ||
                dto.FechaFinNueva.Value ==
                    DateTime.MinValue)
            {
                return "La nueva fecha de fin es obligatoria.";
            }

            if (
                dto.FechaFinNueva.Value.Date <
                dto.FechaInicioNueva.Value.Date
            )
            {
                return "La nueva fecha de fin no puede ser anterior a la fecha de inicio.";
            }

            var motivo = Normalizar(dto.Motivo);

            if (motivo is null)
            {
                return "El motivo de renovación es obligatorio.";
            }

            if (motivo.Length > 500)
            {
                return "El motivo de renovación no puede exceder 500 caracteres.";
            }

            if (dto.ConservarMontoActual &&
                dto.MontoNuevo.HasValue)
            {
                return "No se puede proporcionar un nuevo monto si se indicó conservar el monto actual.";
            }

            if (dto.MontoNuevo.HasValue)
            {
                if (
                    dto.MontoNuevo.Value < 0 ||
                    dto.MontoNuevo.Value >
                        9999999999999999.99m
                )
                {
                    return "El nuevo monto no es válido.";
                }

                if (
                    decimal.Round(
                        dto.MontoNuevo.Value,
                        2) != dto.MontoNuevo.Value
                )
                {
                    return "El nuevo monto admite un máximo de dos decimales.";
                }
            }

            if (
                !dto.FechaInicioEsperada.HasValue ||
                !dto.FechaFinEsperada.HasValue ||
                !dto.UltimaRenovacionEsperada.HasValue ||
                dto.UltimaRenovacionEsperada.Value < 0
            )
            {
                return "No se proporcionó la información de referencia del contrato.";
            }

            return null;
        }

        private static int ObtenerEstadoEfectivo(Contrato contrato, DateTime hoy)
        {
            var estado = contrato.EstadoContratoId;

            if (
                contrato.FechaFin.HasValue &&
                contrato.FechaFin.Value.Date < hoy &&
                (
                    estado == EstadosContratoEnum.Vigente ||
                    estado == EstadosContratoEnum.Suspendido
                )
            )
            {
                return EstadosContratoEnum.Vencido;
            }

            return estado;
        }

        private static string? ValidarContratoParaRenovar(
    Contrato contrato,
    DateTime hoy)
        {
            if (contrato.Activo != true)
            {
                return "No se puede renovar un contrato dado de baja.";
            }

            // La suspensión se conserva como restricción,
            // incluso si la fecha de vigencia ya terminó.

            if (
                contrato.EstadoContratoId ==
                    EstadosContratoEnum.Suspendido
            )
            {
                return "No se puede renovar un contrato suspendido.";
            }

            var estado = ObtenerEstadoEfectivo(
                contrato,
                hoy);

            if (
                estado != EstadosContratoEnum.Vigente &&
                estado != EstadosContratoEnum.Vencido
            )
            {
                return "Solo se pueden renovar contratos vigentes o vencidos.";
            }

            if (!contrato.FechaFin.HasValue)
            {
                return "No se puede renovar un contrato con vigencia indefinida.";
            }

            if (
                estado == EstadosContratoEnum.Vigente &&
                contrato.FechaInicio.Date > hoy
            )
            {
                return "El contrato todavía no ha iniciado su vigencia.";
            }

            if (
                estado == EstadosContratoEnum.Vencido &&
                contrato.FechaFin.Value.Date >= hoy
            )
            {
                return "El estado del contrato no coincide con su fecha de vencimiento.";
            }

            if (
                contrato.FechaFin.Value.Date ==
                    DateTime.MaxValue.Date
            )
            {
                return "No es posible extender la fecha de fin del contrato.";
            }

            return null;
        }

        private static string? ValidarNuevaVigencia(
    Contrato contrato,
    DateTime fechaInicioNueva,
    DateTime fechaFinNueva,
    DateTime hoy)
        {
            if (!contrato.FechaFin.HasValue)
            {
                return "El contrato no tiene una fecha de vencimiento.";
            }

            var fechaInicioAnterior =
                contrato.FechaInicio.Date;

            var fechaFinAnterior =
                contrato.FechaFin.Value.Date;

            var estado = ObtenerEstadoEfectivo(
                contrato,
                hoy);

            if (fechaFinNueva < fechaInicioNueva)
            {
                return "La nueva fecha de fin no puede ser anterior a la fecha de inicio.";
            }

            if (fechaFinNueva <= fechaFinAnterior)
            {
                return "La nueva fecha de fin debe ser posterior a la fecha de fin actual.";
            }

            if (fechaFinNueva < hoy)
            {
                return "La nueva vigencia no puede encontrarse vencida.";
            }

            if (estado == EstadosContratoEnum.Vigente)
            {
                // Una renovación anticipada se maneja como
                // extensión del periodo existente.

                if (fechaInicioNueva != fechaInicioAnterior)
                {
                    return "Para renovar un contrato vigente debes conservar su fecha de inicio actual.";
                }
            }
            else if (estado == EstadosContratoEnum.Vencido)
            {
                // Un contrato vencido puede iniciar un nuevo
                // periodo, sin superponer la vigencia anterior.

                if (fechaInicioNueva <= fechaFinAnterior)
                {
                    return "La nueva fecha de inicio debe ser posterior al vencimiento anterior.";
                }

                if (fechaInicioNueva > hoy)
                {
                    return "La nueva vigencia debe haber iniciado para poder reactivar el contrato.";
                }
            }
            else
            {
                return "El estado actual del contrato no permite renovar.";
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

            if (
                ex is DbUpdateException db &&
                db.InnerException is SqlException inner
            )
            {
                return inner.Number is
                    1205 or
                    2601 or
                    2627 or
                    547;
            }

            if (ex is DbUpdateConcurrencyException)
            {
                return true;
            }

            return false;
        }




    }
}
