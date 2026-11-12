using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto;
using Nextag.Api.Models.Dto.Tickets;
using Nextag.Api.Services.Tickets.Interface;

namespace Nextag.Api.Services.Tickets.Implementation;

public class TicketService : ITicketService
{
    private readonly NextagContext context;
    private readonly ITicketFolioService folioService;

    public TicketService(
        NextagContext context,
        ITicketFolioService folioService)
    {
        this.context = context;
        this.folioService = folioService;
    }

    // ==========================================
    // LISTADO
    // ==========================================

    public async Task<TicketPaginadoDto> GetAllAsync(
        Guid empresaId,
        TicketFiltroDto filtro,
        CancellationToken ct = default)
    {
        var query = context.Tickets
            .AsNoTracking()
            .Where(x => x.EmpresaId == empresaId);

        if (filtro.EstadoTicketId.HasValue)
        {
            query = query.Where(x =>
                x.EstadoTicketId == filtro.EstadoTicketId.Value);
        }

        if (filtro.PrioridadTicketId.HasValue)
        {
            query = query.Where(x =>
                x.PrioridadTicketId == filtro.PrioridadTicketId.Value);
        }

        if (filtro.CategoriaTicketId.HasValue)
        {
            query = query.Where(x =>
                x.CategoriaTicketId == filtro.CategoriaTicketId.Value);
        }

        if (filtro.ActivoId.HasValue)
        {
            query = query.Where(x =>
                x.ActivoId == filtro.ActivoId.Value);
        }

        if (filtro.FechaInicio.HasValue)
        {
            var inicio = filtro.FechaInicio.Value.Date;

            query = query.Where(x =>
                x.FechaApertura >= inicio);
        }

        if (filtro.FechaFin.HasValue)
        {
            var finExclusivo =
                filtro.FechaFin.Value.Date.AddDays(1);

            query = query.Where(x =>
                x.FechaApertura < finExclusivo);
        }

        var buscar = filtro.Buscar?.Trim();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            query = query.Where(x =>
                x.Folio.Contains(buscar) ||
                x.Titulo.Contains(buscar) ||
                x.Descripcion.Contains(buscar) ||
                (x.Activo != null &&
                 x.Activo.Codigo.Contains(buscar)));
        }

        var total = await query.CountAsync(ct);

        var pagina = Math.Max(filtro.Pagina, 1);
        var tamano = Math.Clamp(filtro.TamanoPagina, 1, 100);

        var items = await query
            .OrderByDescending(x => x.FechaApertura)
            .ThenByDescending(x => x.Numero)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(x => new TicketListDto
            {
                Id = x.Id,
                EmpresaId = x.EmpresaId,

                Numero = x.Numero,
                Folio = x.Folio,

                ActivoId = x.ActivoId,

                ActivoCodigo = x.Activo != null
                    ? x.Activo.Codigo
                    : null,

                ActivoNombre = x.Activo != null
                    ? x.Activo.Nombre
                    : null,

                CategoriaTicketId = x.CategoriaTicketId,

                CategoriaNombre = x.CategoriaTicket != null
                    ? x.CategoriaTicket.Nombre
                    : null,

                PrioridadTicketId = x.PrioridadTicketId,
                PrioridadNombre = x.PrioridadTicket.Nombre,

                EstadoTicketId = x.EstadoTicketId,
                EstadoNombre = x.EstadoTicket.Nombre,

                Origen = x.Origen,

                Titulo = x.Titulo,

                SolicitanteUsuarioId = x.SolicitanteUsuarioId,
                SolicitanteNombre = x.SolicitanteNombre,

                FechaApertura = x.FechaApertura
            })
            .ToListAsync(ct);

        return new TicketPaginadoDto
        {
            Items = items,
            TotalRegistros = total,
            Pagina = pagina,
            TamanoPagina = tamano,
            TotalPaginas = (int)Math.Ceiling(
                total / (double)tamano)
        };
    }

    // ==========================================
    // DETALLE
    // ==========================================

    public async Task<TicketDetalleDto?> GetByIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken ct = default)
    {
        return await context.Tickets
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.EmpresaId == empresaId)
            .Select(x => new TicketDetalleDto
            {
                Id = x.Id,
                EmpresaId = x.EmpresaId,

                Numero = x.Numero,
                Folio = x.Folio,

                ActivoId = x.ActivoId,

                ActivoCodigo = x.Activo != null
                    ? x.Activo.Codigo
                    : null,

                ActivoNombre = x.Activo != null
                    ? x.Activo.Nombre
                    : null,

                CategoriaTicketId = x.CategoriaTicketId,

                CategoriaNombre = x.CategoriaTicket != null
                    ? x.CategoriaTicket.Nombre
                    : null,

                PrioridadTicketId = x.PrioridadTicketId,
                PrioridadNombre = x.PrioridadTicket.Nombre,

                EstadoTicketId = x.EstadoTicketId,
                EstadoNombre = x.EstadoTicket.Nombre,

                Origen = x.Origen,

                Titulo = x.Titulo,
                Descripcion = x.Descripcion,

                SolicitanteUsuarioId = x.SolicitanteUsuarioId,
                SolicitanteNombre = x.SolicitanteNombre,
                SolicitanteCorreo = x.SolicitanteCorreo,
                SolicitanteTelefono = x.SolicitanteTelefono,

                FechaApertura = x.FechaApertura,
                FechaPrimeraAtencion = x.FechaPrimeraAtencion,
                FechaResolucion = x.FechaResolucion,
                FechaCierre = x.FechaCierre,
                FechaCancelacion = x.FechaCancelacion,
                MotivoCancelacion = x.MotivoCancelacion,

                FechaCreacion = x.FechaCreacion,
                UsuarioCreacion = x.UsuarioCreacion,
                FechaActualizacion = x.FechaActualizacion,
                UsuarioModificacion = x.UsuarioModificacion
            })
            .FirstOrDefaultAsync(ct);
    }

    // ==========================================
    // TICKETS POR ACTIVO
    // ==========================================

    public async Task<List<TicketListDto>> GetByActivoAsync(
        Guid activoId,
        Guid empresaId,
        CancellationToken ct = default)
    {
        return await context.Tickets
            .AsNoTracking()
            .Where(x =>
                x.EmpresaId == empresaId &&
                x.ActivoId == activoId)
            .OrderByDescending(x => x.FechaApertura)
            .Select(x => new TicketListDto
            {
                Id = x.Id,
                EmpresaId = x.EmpresaId,

                Numero = x.Numero,
                Folio = x.Folio,

                ActivoId = x.ActivoId,

                ActivoCodigo = x.Activo != null
                    ? x.Activo.Codigo
                    : null,

                ActivoNombre = x.Activo != null
                    ? x.Activo.Nombre
                    : null,

                CategoriaTicketId = x.CategoriaTicketId,

                CategoriaNombre = x.CategoriaTicket != null
                    ? x.CategoriaTicket.Nombre
                    : null,

                PrioridadTicketId = x.PrioridadTicketId,
                PrioridadNombre = x.PrioridadTicket.Nombre,

                EstadoTicketId = x.EstadoTicketId,
                EstadoNombre = x.EstadoTicket.Nombre,

                Origen = x.Origen,
                Titulo = x.Titulo,

                SolicitanteUsuarioId = x.SolicitanteUsuarioId,
                SolicitanteNombre = x.SolicitanteNombre,

                FechaApertura = x.FechaApertura
            })
            .ToListAsync(ct);
    }

    // ==========================================
    // CATALOGO ESTADOS
    // ==========================================

    public async Task<List<TicketCatalogoDto>> GetEstadosAsync(
        CancellationToken ct = default)
    {
        return await context.EstadoTickets
            .AsNoTracking()
            .Where(x => x.Activo == true)
            .OrderBy(x => x.Orden)
            .Select(x => new TicketCatalogoDto
            {
                Id = x.Id,
                Clave = x.Clave,
                Nombre = x.Nombre,
                Orden = x.Orden
            })
            .ToListAsync(ct);
    }

    // ==========================================
    // CATALOGO PRIORIDADES
    // ==========================================

    public async Task<List<TicketCatalogoDto>> GetPrioridadesAsync(
        CancellationToken ct = default)
    {
        return await context.PrioridadTickets
            .AsNoTracking()
            .Where(x => x.Activo == true)
            .OrderBy(x => x.Orden)
            .Select(x => new TicketCatalogoDto
            {
                Id = x.Id,
                Clave = x.Clave,
                Nombre = x.Nombre,
                Orden = x.Orden
            })
            .ToListAsync(ct);
    }

    // ==========================================
    // ACTUALIZAR TICKET
    // ==========================================

    public async Task<ServiceResult<TicketDetalleDto>> UpdateAsync(
        Guid id,
        TicketActualizarDto dto,
        Guid empresaId,
        Guid usuarioId,
        CancellationToken ct = default)
    {
        var ticket = await context.Tickets
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.EmpresaId == empresaId,
                ct);

        if (ticket == null)
        {
            return ServiceResult<TicketDetalleDto>.Fail(
                "Ticket no encontrado."
            );
        }

        // En esta entrega solo editaremos
        // tickets que permanecen abiertos.

        var estado = await context.EstadoTickets
            .AsNoTracking()
            .Where(x => x.Id == ticket.EstadoTicketId)
            .Select(x => x.Clave)
            .FirstOrDefaultAsync(ct);

        if (estado != "ABIERTO")
        {
            return ServiceResult<TicketDetalleDto>.Fail(
                "Solamente pueden modificarse tickets en estado ABIERTO."
            );
        }

        var error = await ValidarDatosAsync(
            dto.Titulo,
            dto.Descripcion,
            dto.CategoriaTicketId,
            dto.PrioridadTicketId,
            dto.ActivoId,
            empresaId,
            ct);

        if (error != null)
        {
            return ServiceResult<TicketDetalleDto>.Fail(error);
        }

        ticket.Titulo = dto.Titulo.Trim();
        ticket.Descripcion = dto.Descripcion.Trim();

        ticket.CategoriaTicketId = dto.CategoriaTicketId;
        ticket.PrioridadTicketId = dto.PrioridadTicketId;
        ticket.ActivoId = dto.ActivoId;

        ticket.FechaActualizacion = DateTime.UtcNow;
        ticket.UsuarioModificacion = usuarioId;

        await context.SaveChangesAsync(ct);

        var detalle = await GetByIdAsync(
            id,
            empresaId,
            ct);

        return ServiceResult<TicketDetalleDto>.Ok(detalle!);
    }

    // ==========================================
    // CREAR TICKET
    // ==========================================

    public async Task<ServiceResult<TicketDetalleDto>> CreateAsync(
        TicketCrearDto dto,
        Guid empresaId,
        Guid usuarioId,
        CancellationToken ct = default)
    {
        var error = await ValidarDatosAsync(
            dto.Titulo,
            dto.Descripcion,
            dto.CategoriaTicketId,
            dto.PrioridadTicketId,
            dto.ActivoId,
            empresaId,
            ct);

        if (error != null)
        {
            return ServiceResult<TicketDetalleDto>.Fail(error);
        }

        // Validar estado inicial por clave,
        // evitando depender de un ID fijo.

        var estadoAbierto = await context.EstadoTickets
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Clave == "ABIERTO" &&
                x.Activo == true,
                ct);

        if (estadoAbierto == null)
        {
            return ServiceResult<TicketDetalleDto>.Fail(
                "No se encuentra configurado el estado ABIERTO."
            );
        }

        // Obtener el usuario solicitante.

        var solicitante = await context.Usuarios
            .AsNoTracking()
            .Where(x => x.Id == usuarioId)
            .Select(x => new
            {
                x.Id,
                x.Nombre,
                x.Apellidos,
                x.Email,
                x.Telefono
            })
            .FirstOrDefaultAsync(ct);

        if (solicitante == null)
        {
            return ServiceResult<TicketDetalleDto>.Fail(
                "No se encontró el usuario solicitante."
            );
        }

        // ======================================
        // TRANSACCION
        // ======================================

        await using var transaction =
            await context.Database.BeginTransactionAsync(ct);

        // ======================================
        // GENERAR FOLIO
        // ======================================

        var numero = await folioService.SiguienteAsync(
            empresaId,
            ct);

        var folio = $"TIC-{numero:D6}";

        var ahora = DateTime.UtcNow;


        // ======================================
        // REGISTRO DEL TICKET
        // ======================================

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),

            EmpresaId = empresaId,

            Numero = numero,
            Folio = folio,

            ActivoId = dto.ActivoId,
            CategoriaTicketId = dto.CategoriaTicketId,

            PrioridadTicketId = dto.PrioridadTicketId,

            EstadoTicketId = estadoAbierto.Id,

            Origen = "INTERNO",

            Titulo = dto.Titulo.Trim(),
            Descripcion = dto.Descripcion.Trim(),

            SolicitanteUsuarioId = usuarioId,

            SolicitanteNombre =
                $"{solicitante.Nombre} {solicitante.Apellidos}".Trim(),

            SolicitanteCorreo = solicitante.Email,
            SolicitanteTelefono = solicitante.Telefono,

            FechaApertura = ahora,

            FechaCreacion = ahora,
            UsuarioCreacion = usuarioId
        };

        context.Tickets.Add(ticket);

        // ======================================
        // HISTORIAL INICIAL
        // ======================================

        var historial = new TicketEstadoHistorial
        {
            Id = Guid.NewGuid(),

            EmpresaId = empresaId,
            TicketId = ticket.Id,

            EstadoAnteriorId = null,
            EstadoNuevoId = estadoAbierto.Id,

            Motivo = "Creación del ticket.",

            FechaCambio = ahora,
            UsuarioId = usuarioId
        };

        context.TicketEstadoHistorials.Add(historial);

        // Se guardan contador, ticket e historial
        // dentro de la misma transacción.

        await context.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);

        var detalle = await GetByIdAsync(
            ticket.Id,
            empresaId,
            ct);

        return ServiceResult<TicketDetalleDto>.Ok(detalle!);
    }

    // ==========================================
    // VALIDAR DATOS
    // ==========================================

    private async Task<string?> ValidarDatosAsync(
        string? titulo,
        string? descripcion,
        Guid? categoriaTicketId,
        int prioridadTicketId,
        Guid? activoId,
        Guid empresaId,
        CancellationToken ct)
    {
        // TITULO

        if (string.IsNullOrWhiteSpace(titulo))
        {
            return "El título del ticket es obligatorio.";
        }

        if (titulo.Trim().Length > 200)
        {
            return "El título no puede superar 200 caracteres.";
        }

        // DESCRIPCION

        if (string.IsNullOrWhiteSpace(descripcion))
        {
            return "La descripción del ticket es obligatoria.";
        }

        // PRIORIDAD

        var prioridadValida = await context.PrioridadTickets
            .AsNoTracking()
            .AnyAsync(x =>
                x.Id == prioridadTicketId &&
                x.Activo == true,
                ct);

        if (!prioridadValida)
        {
            return "La prioridad seleccionada no es válida.";
        }

        // CATEGORIA

        if (categoriaTicketId.HasValue)
        {
            if (categoriaTicketId.Value == Guid.Empty)
            {
                return "La categoría seleccionada no es válida.";
            }

            var categoriaValida = await context.CategoriaTickets
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == categoriaTicketId.Value &&
                    x.EmpresaId == empresaId &&
                    x.Activo == true,
                    ct);

            if (!categoriaValida)
            {
                return "La categoría no existe, está inactiva o pertenece a otra empresa.";
            }
        }

        // ACTIVO

        if (activoId.HasValue)
        {
            if (activoId.Value == Guid.Empty)
            {
                return "El activo seleccionado no es válido.";
            }

            var activoExiste = await context.Activos
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == activoId.Value &&
                    x.EmpresaId == empresaId,
                    ct);

            if (!activoExiste)
            {
                return "El activo no existe o pertenece a otra empresa.";
            }
        }

        return null;
    }
}