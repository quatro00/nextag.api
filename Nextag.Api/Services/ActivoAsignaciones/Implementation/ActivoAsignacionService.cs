using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto.ActivoAsignaciones;
using Nextag.Api.Models.Dto;
using Nextag.Api.Services.ActivoAsignaciones.Interface;

namespace Nextag.Api.Services.ActivoAsignaciones.Implementation
{
    public class ActivoAsignacionService : IActivoAsignacionService
    {
        private const int EstadoBaja = 7;

        private readonly NextagContext context;

        public ActivoAsignacionService(NextagContext context)
        {
            this.context = context;
        }

        // ==================================================
        // CONSULTA BASE
        // ==================================================

        private IQueryable<ActivoAsignacionDto> Consulta(
            Guid empresaId)
        {
            return context.ActivoAsignacions
                .AsNoTracking()
                .Where(x => x.EmpresaId == empresaId)
                .Select(x => new ActivoAsignacionDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,

                    // Activo
                    ActivoId = x.ActivoId,
                    ActivoCodigo = x.Activo.Codigo,
                    ActivoNombre = x.Activo.Nombre,

                    ActivoNumeroSerie = x.Activo.NumeroSerie,
                    ActivoNumeroInventario =
                        x.Activo.NumeroInventario,

                    SucursalId = x.Activo.SucursalId,
                    SucursalNombre = x.Activo.Sucursal.Nombre,

                    UbicacionId = x.Activo.UbicacionId,

                    UbicacionNombre =
                        x.Activo.Ubicacion != null
                            ? x.Activo.Ubicacion.Nombre
                            : null,

                    // Responsable
                    ResponsableId = x.ResponsableId,

                    ResponsableNombre = x.Responsable.Nombre,

                    ResponsableTipo = x.Responsable.Tipo,

                    ResponsableCodigo = x.Responsable.Codigo,

                    AreaResponsableId =
                        x.Responsable.AreaResponsableId,

                    AreaResponsableNombre =
                        x.Responsable.ResponsableNavigation != null
                            ? x.Responsable
                                .ResponsableNavigation.Nombre
                            : null,

                    // Asignación
                    FechaAsignacion = x.FechaAsignacion,

                    MotivoAsignacion = x.MotivoAsignacion,

                    ObservacionesAsignacion =
                        x.ObservacionesAsignacion,

                    // Devolución
                    FechaDevolucion = x.FechaDevolucion,

                    MotivoDevolucion = x.MotivoDevolucion,

                    CondicionDevolucion =
                        x.CondicionDevolucion,

                    ObservacionesDevolucion =
                        x.ObservacionesDevolucion,

                    UsuarioDevolucion = x.UsuarioDevolucion,

                    Vigente = x.FechaDevolucion == null,

                    // Auditoría
                    FechaCreacion = x.FechaCreacion,

                    UsuarioCreacion = x.UsuarioCreacion,

                    FechaActualizacion = x.FechaActualizacion,

                    UsuarioModificacion = x.UsuarioModificacion
                });
        }

        // ==================================================
        // LISTADO PAGINADO
        // ==================================================

        public async Task<ActivoAsignacionPaginaDto> GetAllAsync(
            Guid empresaId,
            ActivoAsignacionFiltroDto filtros,
            CancellationToken ct = default)
        {
            var pagina = Math.Max(1, filtros.Pagina);

            var tamano = Math.Clamp(
                filtros.TamanoPagina,
                1,
                100);

            var query = Consulta(empresaId);

            if (filtros.ActivoId.HasValue)
            {
                query = query.Where(x =>
                    x.ActivoId == filtros.ActivoId.Value);
            }

            if (filtros.ResponsableId.HasValue)
            {
                query = query.Where(x =>
                    x.ResponsableId == filtros.ResponsableId.Value);
            }

            if (filtros.SucursalId.HasValue)
            {
                query = query.Where(x =>
                    x.SucursalId == filtros.SucursalId.Value);
            }

            if (!string.IsNullOrWhiteSpace(
                filtros.TipoResponsable))
            {
                var tipo = filtros.TipoResponsable
                    .Trim()
                    .ToUpperInvariant();

                query = query.Where(x =>
                    x.ResponsableTipo == tipo);
            }

            if (filtros.Vigente.HasValue)
            {
                query = query.Where(x =>
                    x.Vigente == filtros.Vigente.Value);
            }

            if (filtros.FechaDesde.HasValue)
            {
                query = query.Where(x =>
                    x.FechaAsignacion >=
                    filtros.FechaDesde.Value);
            }

            if (filtros.FechaHasta.HasValue)
            {
                query = query.Where(x =>
                    x.FechaAsignacion <=
                    filtros.FechaHasta.Value);
            }

            if (!string.IsNullOrWhiteSpace(filtros.Buscar))
            {
                var buscar = filtros.Buscar.Trim();

                query = query.Where(x =>
                    x.ActivoCodigo.Contains(buscar) ||
                    x.ActivoNombre.Contains(buscar) ||
                    x.ResponsableNombre.Contains(buscar) ||
                    (
                        x.ResponsableCodigo != null &&
                        x.ResponsableCodigo.Contains(buscar)
                    ) ||
                    (
                        x.ActivoNumeroSerie != null &&
                        x.ActivoNumeroSerie.Contains(buscar)
                    )
                );
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.FechaAsignacion)
                .ThenByDescending(x => x.Id)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync(ct);

            return new ActivoAsignacionPaginaDto
            {
                Items = items,
                Total = total,
                Pagina = pagina,
                TamanoPagina = tamano,

                TotalPaginas = (int)Math.Ceiling(
                    total / (double)tamano)
            };
        }

        // ==================================================
        // DETALLE
        // ==================================================

        public Task<ActivoAsignacionDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default)
        {
            return Consulta(empresaId)
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    ct);
        }

        // ==================================================
        // HISTORIAL DE ACTIVO
        // ==================================================

        public Task<List<ActivoAsignacionDto>>
            GetHistorialActivoAsync(
                Guid activoId,
                Guid empresaId,
                CancellationToken ct = default)
        {
            return Consulta(empresaId)
                .Where(x => x.ActivoId == activoId)
                .OrderByDescending(x => x.FechaAsignacion)
                .ThenByDescending(x => x.Id)
                .ToListAsync(ct);
        }

        // ==================================================
        // HISTORIAL DE RESPONSABLE
        // ==================================================

        public Task<List<ActivoAsignacionDto>>
            GetHistorialResponsableAsync(
                Guid responsableId,
                Guid empresaId,
                CancellationToken ct = default)
        {
            return Consulta(empresaId)
                .Where(x => x.ResponsableId == responsableId)
                .OrderByDescending(x => x.FechaAsignacion)
                .ThenByDescending(x => x.Id)
                .ToListAsync(ct);
        }

        // ==================================================
        // ASIGNACION VIGENTE DE UN ACTIVO
        // ==================================================

        public Task<ActivoAsignacionDto?> GetVigenteActivoAsync(
            Guid activoId,
            Guid empresaId,
            CancellationToken ct = default)
        {
            return Consulta(empresaId)
                .FirstOrDefaultAsync(x =>
                    x.ActivoId == activoId &&
                    x.Vigente,
                    ct);
        }

        // ==================================================
        // ACTIVOS SIN ASIGNAR
        // ==================================================

        public async Task<ActivosSinAsignarPaginaDto>
            GetActivosSinAsignarAsync(
                Guid empresaId,
                string? buscar,
                Guid? sucursalId,
                int pagina,
                int tamanoPagina,
                CancellationToken ct = default)
        {
            pagina = Math.Max(1, pagina);
            tamanoPagina = Math.Clamp(tamanoPagina, 1, 100);

            var query = context.Activos
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.EstadoActivoId != EstadoBaja &&
                    x.FechaBaja == null &&
                    !context.ActivoAsignacions.Any(a =>
                        a.EmpresaId == empresaId &&
                        a.ActivoId == x.Id &&
                        a.FechaDevolucion == null));

            if (sucursalId.HasValue)
            {
                query = query.Where(x =>
                    x.SucursalId == sucursalId.Value);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();

                query = query.Where(x =>
                    x.Codigo.Contains(texto) ||
                    x.Nombre.Contains(texto) ||
                    (
                        x.NumeroSerie != null &&
                        x.NumeroSerie.Contains(texto)
                    ) ||
                    (
                        x.NumeroInventario != null &&
                        x.NumeroInventario.Contains(texto)
                    ));
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderBy(x => x.Codigo)
                .ThenBy(x => x.Id)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .Select(x => new ActivoSinAsignarDto
                {
                    Id = x.Id,
                    Codigo = x.Codigo,
                    Nombre = x.Nombre,

                    NumeroSerie = x.NumeroSerie,
                    NumeroInventario = x.NumeroInventario,

                    SucursalId = x.SucursalId,
                    SucursalNombre = x.Sucursal.Nombre,

                    UbicacionId = x.UbicacionId,

                    UbicacionNombre =
                        x.Ubicacion != null
                            ? x.Ubicacion.Nombre
                            : null,

                    EstadoActivoId = x.EstadoActivoId,

                    EstadoActivoNombre = x.EstadoActivo.Nombre
                })
                .ToListAsync(ct);

            return new ActivosSinAsignarPaginaDto
            {
                Items = items,
                Total = total,
                Pagina = pagina,
                TamanoPagina = tamanoPagina,

                TotalPaginas = (int)Math.Ceiling(
                    total / (double)tamanoPagina)
            };
        }

        // ==================================================
        // CREAR ASIGNACION
        // ==================================================

        public async Task<ServiceResult<ActivoAsignacionDto>>
            CreateAsync(
                ActivoAsignacionCrearDto dto,
                Guid empresaId,
                Guid usuarioId,
                CancellationToken ct = default)
        {
            if (dto.ActivoId == Guid.Empty)
            {
                return ServiceResult<ActivoAsignacionDto>
                    .Fail("Selecciona un activo.");
            }

            if (dto.ResponsableId == Guid.Empty)
            {
                return ServiceResult<ActivoAsignacionDto>
                    .Fail("Selecciona un responsable.");
            }

            var fecha = dto.FechaAsignacion?.UtcDateTime
                        ?? DateTime.UtcNow;

            var errorFecha = ValidarFechaOperacion(fecha);

            if (errorFecha != null)
            {
                return ServiceResult<ActivoAsignacionDto>
                    .Fail(errorFecha);
            }

            await using var transaction =
                await context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    ct);

            try
            {
                var errorActivo = await ValidarActivoAsync(
                    dto.ActivoId,
                    empresaId,
                    ct);

                if (errorActivo != null)
                {
                    return ServiceResult<ActivoAsignacionDto>
                        .Fail(errorActivo);
                }

                var errorResponsable =
                    await ValidarResponsableAsync(
                        dto.ResponsableId,
                        empresaId,
                        ct);

                if (errorResponsable != null)
                {
                    return ServiceResult<ActivoAsignacionDto>
                        .Fail(errorResponsable);
                }

                // Verificar asignación vigente dentro de
                // la transacción Serializable.

                var tieneAsignacion = await context
                    .ActivoAsignacions
                    .AnyAsync(x =>
                        x.EmpresaId == empresaId &&
                        x.ActivoId == dto.ActivoId &&
                        x.FechaDevolucion == null,
                        ct);

                if (tieneAsignacion)
                {
                    return ServiceResult<ActivoAsignacionDto>
                        .Fail(
                            "El activo ya tiene una asignación vigente.");
                }

                // Evitar insertar fechas anteriores al cierre
                // de la asignación histórica más reciente.

                var ultimaDevolucion = await context
                    .ActivoAsignacions
                    .Where(x =>
                        x.EmpresaId == empresaId &&
                        x.ActivoId == dto.ActivoId)
                    .OrderByDescending(x => x.FechaAsignacion)
                    .ThenByDescending(x => x.Id)
                    .Select(x => x.FechaDevolucion)
                    .FirstOrDefaultAsync(ct);

                if (ultimaDevolucion.HasValue &&
                    fecha < ultimaDevolucion.Value)
                {
                    return ServiceResult<ActivoAsignacionDto>
                        .Fail(
                            "La fecha de asignación no puede ser " +
                            "anterior a la última devolución.");
                }

                var asignacion = new ActivoAsignacion
                {
                    EmpresaId = empresaId,

                    ActivoId = dto.ActivoId,
                    ResponsableId = dto.ResponsableId,

                    FechaAsignacion = fecha,

                    MotivoAsignacion =
                        Normalizar(dto.MotivoAsignacion),

                    ObservacionesAsignacion =
                        Normalizar(dto.ObservacionesAsignacion),

                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacion = usuarioId
                };

                context.ActivoAsignacions.Add(asignacion);

                await context.SaveChangesAsync(ct);

                await transaction.CommitAsync(ct);

                var detalle = await GetByIdAsync(
                    asignacion.Id,
                    empresaId,
                    ct);

                return ServiceResult<ActivoAsignacionDto>
                    .Ok(detalle!);
            }
            catch (Exception ex) when (EsConflictoConcurrencia(ex))
            {
                return ServiceResult<ActivoAsignacionDto>
                    .Fail(
                        "El activo está siendo actualizado por " +
                        "otro usuario. Intenta nuevamente.");
            }
        }

        // ==================================================
        // DEVOLVER ACTIVO
        // ==================================================

        public async Task<ServiceResult<ActivoAsignacionDto>>
            DevolverAsync(
                Guid id,
                ActivoAsignacionDevolverDto dto,
                Guid empresaId,
                Guid usuarioId,
                CancellationToken ct = default)
        {
            var fecha = dto.FechaDevolucion?.UtcDateTime
                        ?? DateTime.UtcNow;

            var errorFecha = ValidarFechaOperacion(fecha);

            if (errorFecha != null)
            {
                return ServiceResult<ActivoAsignacionDto>
                    .Fail(errorFecha);
            }

            await using var transaction =
                await context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    ct);

            try
            {
                var asignacion = await context.ActivoAsignacions
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        x.EmpresaId == empresaId,
                        ct);

                if (asignacion == null)
                {
                    return ServiceResult<ActivoAsignacionDto>
                        .Fail("Asignación no encontrada.");
                }

                if (asignacion.FechaDevolucion.HasValue)
                {
                    return ServiceResult<ActivoAsignacionDto>
                        .Fail(
                            "Esta asignación ya fue finalizada.");
                }

                if (fecha < asignacion.FechaAsignacion)
                {
                    return ServiceResult<ActivoAsignacionDto>
                        .Fail(
                            "La fecha de devolución no puede " +
                            "ser anterior a la asignación.");
                }

                asignacion.FechaDevolucion = fecha;

                asignacion.MotivoDevolucion =
                    Normalizar(dto.MotivoDevolucion);

                asignacion.CondicionDevolucion =
                    Normalizar(dto.CondicionDevolucion);

                asignacion.ObservacionesDevolucion =
                    Normalizar(dto.ObservacionesDevolucion);

                asignacion.UsuarioDevolucion = usuarioId;

                asignacion.FechaActualizacion = DateTime.UtcNow;
                asignacion.UsuarioModificacion = usuarioId;

                await context.SaveChangesAsync(ct);

                await transaction.CommitAsync(ct);

                var detalle = await GetByIdAsync(
                    asignacion.Id,
                    empresaId,
                    ct);

                return ServiceResult<ActivoAsignacionDto>
                    .Ok(detalle!);
            }
            catch (Exception ex) when (EsConflictoConcurrencia(ex))
            {
                return ServiceResult<ActivoAsignacionDto>
                    .Fail(
                        "La asignación está siendo actualizada " +
                        "por otro usuario. Intenta nuevamente.");
            }
        }

        // ==================================================
        // REASIGNAR ACTIVO
        // ==================================================

        public async Task<ServiceResult<ActivoAsignacionDto>>
            ReasignarAsync(
                Guid id,
                ActivoAsignacionReasignarDto dto,
                Guid empresaId,
                Guid usuarioId,
                CancellationToken ct = default)
        {
            if (dto.NuevoResponsableId == Guid.Empty)
            {
                return ServiceResult<ActivoAsignacionDto>
                    .Fail("Selecciona el nuevo responsable.");
            }

            var fecha = dto.FechaReasignacion?.UtcDateTime
                        ?? DateTime.UtcNow;

            var errorFecha = ValidarFechaOperacion(fecha);

            if (errorFecha != null)
            {
                return ServiceResult<ActivoAsignacionDto>
                    .Fail(errorFecha);
            }

            await using var transaction =
                await context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    ct);

            try
            {
                var anterior = await context.ActivoAsignacions
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        x.EmpresaId == empresaId,
                        ct);

                if (anterior == null)
                {
                    return ServiceResult<ActivoAsignacionDto>
                        .Fail("Asignación no encontrada.");
                }

                if (anterior.FechaDevolucion.HasValue)
                {
                    return ServiceResult<ActivoAsignacionDto>
                        .Fail(
                            "Solo se pueden reasignar " +
                            "asignaciones vigentes.");
                }

                if (anterior.ResponsableId ==
                    dto.NuevoResponsableId)
                {
                    return ServiceResult<ActivoAsignacionDto>
                        .Fail(
                            "El nuevo responsable debe ser " +
                            "diferente del actual.");
                }

                if (fecha < anterior.FechaAsignacion)
                {
                    return ServiceResult<ActivoAsignacionDto>
                        .Fail(
                            "La fecha de reasignación no puede " +
                            "ser anterior a la asignación actual.");
                }

                var errorActivo = await ValidarActivoAsync(
                    anterior.ActivoId,
                    empresaId,
                    ct);

                if (errorActivo != null)
                {
                    return ServiceResult<ActivoAsignacionDto>
                        .Fail(errorActivo);
                }

                var errorResponsable =
                    await ValidarResponsableAsync(
                        dto.NuevoResponsableId,
                        empresaId,
                        ct);

                if (errorResponsable != null)
                {
                    return ServiceResult<ActivoAsignacionDto>
                        .Fail(errorResponsable);
                }

                // Confirmar que no exista una asignación
                // vigente adicional por inconsistencias previas.

                var otraVigente = await context
                    .ActivoAsignacions
                    .AnyAsync(x =>
                        x.EmpresaId == empresaId &&
                        x.ActivoId == anterior.ActivoId &&
                        x.FechaDevolucion == null &&
                        x.Id != anterior.Id,
                        ct);

                if (otraVigente)
                {
                    return ServiceResult<ActivoAsignacionDto>
                        .Fail(
                            "El activo presenta más de una " +
                            "asignación vigente. Revisa su historial.");
                }

                var ahora = DateTime.UtcNow;

                // Finalizar asignación anterior.

                anterior.FechaDevolucion = fecha;

                anterior.MotivoDevolucion =
                    Normalizar(dto.Motivo);

                anterior.CondicionDevolucion =
                    Normalizar(dto.CondicionDevolucion);

                anterior.ObservacionesDevolucion =
                    Normalizar(dto.ObservacionesDevolucion);

                anterior.UsuarioDevolucion = usuarioId;

                anterior.FechaActualizacion = ahora;
                anterior.UsuarioModificacion = usuarioId;

                // Crear nueva asignación.

                var nueva = new ActivoAsignacion
                {
                    EmpresaId = empresaId,

                    ActivoId = anterior.ActivoId,

                    ResponsableId = dto.NuevoResponsableId,

                    FechaAsignacion = fecha,

                    MotivoAsignacion =
                        Normalizar(dto.Motivo),

                    ObservacionesAsignacion =
                        Normalizar(dto.ObservacionesAsignacion),

                    FechaCreacion = ahora,
                    UsuarioCreacion = usuarioId
                };

                context.ActivoAsignacions.Add(nueva);

                await context.SaveChangesAsync(ct);

                await transaction.CommitAsync(ct);

                var detalle = await GetByIdAsync(
                    nueva.Id,
                    empresaId,
                    ct);

                return ServiceResult<ActivoAsignacionDto>
                    .Ok(detalle!);
            }
            catch (Exception ex) when (EsConflictoConcurrencia(ex))
            {
                return ServiceResult<ActivoAsignacionDto>
                    .Fail(
                        "Otro usuario modificó la asignación " +
                        "durante la operación. Intenta nuevamente.");
            }
        }

        // ==================================================
        // VALIDAR ACTIVO
        // ==================================================

        private async Task<string?> ValidarActivoAsync(
            Guid activoId,
            Guid empresaId,
            CancellationToken ct)
        {
            var activo = await context.Activos
                .AsNoTracking()
                .Where(x =>
                    x.Id == activoId &&
                    x.EmpresaId == empresaId)
                .Select(x => new
                {
                    x.Id,
                    x.EstadoActivoId,
                    x.FechaBaja
                })
                .FirstOrDefaultAsync(ct);

            if (activo == null)
            {
                return "El activo no existe en la empresa.";
            }

            if (activo.EstadoActivoId == EstadoBaja ||
                activo.FechaBaja.HasValue)
            {
                return "No se pueden asignar activos dados de baja.";
            }

            return null;
        }

        // ==================================================
        // VALIDAR RESPONSABLE
        // ==================================================

        private async Task<string?> ValidarResponsableAsync(
            Guid responsableId,
            Guid empresaId,
            CancellationToken ct)
        {
            var responsable = await context.Responsables
                .AsNoTracking()
                .Where(x =>
                    x.Id == responsableId &&
                    x.EmpresaId == empresaId)
                .Select(x => new
                {
                    x.Id,
                    x.Tipo,
                    x.Activo
                })
                .FirstOrDefaultAsync(ct);

            if (responsable == null)
            {
                return "El responsable no existe en la empresa.";
            }

            if (responsable.Activo != true)
            {
                return "El responsable se encuentra inactivo.";
            }

            if (responsable.Tipo != "PERSONA" &&
                responsable.Tipo != "AREA")
            {
                return "El tipo de responsable no es válido.";
            }

            return null;
        }

        // ==================================================
        // HELPERS
        // ==================================================

        private static string? Normalizar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? null
                : valor.Trim();
        }

        private static string? ValidarFechaOperacion(DateTime fecha)
        {
            if (fecha > DateTime.UtcNow.AddMinutes(1))
            {
                return "No se permiten fechas futuras.";
            }

            return null;
        }

        private static bool EsConflictoConcurrencia(Exception ex)
        {
            if (ex is DbUpdateConcurrencyException)
            {
                return true;
            }

            if (ex is SqlException sql)
            {
                return sql.Number is 1205 or 1222;
            }

            if (ex is DbUpdateException db &&
                db.InnerException is SqlException interno)
            {
                return interno.Number is 1205 or 1222;
            }

            return false;
        }
    }
}
