using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Dto.Notificaciones;
using Nextag.Api.Services.Notificaciones.Interface;

namespace Nextag.Api.Services.Notificaciones.Implementation
{
    public class NotificacionInternaService
        : INotificacionInternaService
    {
        private readonly NextagContext context;

        public NotificacionInternaService(
            NextagContext context)
        {
            this.context = context;
        }

        // ==========================================
        // LISTAR NOTIFICACIONES
        // ==========================================

        public async Task<NotificacionPaginadoDto> ObtenerAsync(
            Guid empresaId,
            Guid usuarioId,
            NotificacionConsultaDto consulta,
            CancellationToken ct = default)
        {
            if (consulta == null)
            {
                throw new ArgumentNullException(
                    nameof(consulta));
            }

            // ======================================
            // PAGINACION
            // ======================================

            var pagina = Math.Max(
                consulta.Pagina,
                1);

            var tamanoPagina = Math.Clamp(
                consulta.TamanoPagina,
                1,
                100);

            // ======================================
            // CONSULTA BASE
            // ======================================

            var query = context.Notificacions
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.UsuarioId == usuarioId &&
                    x.TipoDestinatario == "USUARIO" &&
                    x.MostrarInterna);

            // ======================================
            // FILTRAR NO LEIDAS
            // ======================================

            if (consulta.SoloNoLeidas == true)
            {
                query = query.Where(x =>
                    x.FechaLectura == null);
            }

            // ======================================
            // TOTAL
            // ======================================

            var total = await query.CountAsync(ct);

            // ======================================
            // OBTENER REGISTROS
            // ======================================

            var items = await query
                .OrderByDescending(x => x.FechaCreacion)
                .ThenByDescending(x => x.Id)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .Select(x => new NotificacionListaDto
                {
                    Id = x.Id,

                    EmpresaId = x.EmpresaId,

                    TicketId = x.TicketId,

                    EventoClave =
                        x.E.NotificacionEvento.Clave,

                    Titulo = x.Titulo,

                    Mensaje = x.Mensaje,

                    RutaNavegacion = x.RutaNavegacion,

                    FechaCreacion = x.FechaCreacion,

                    FechaLectura = x.FechaLectura
                })
                .ToListAsync(ct);

            // ======================================
            // RESULTADO
            // ======================================

            return new NotificacionPaginadoDto
            {
                Items = items,

                TotalRegistros = total,

                Pagina = pagina,

                TamanoPagina = tamanoPagina,

                TotalPaginas = (int)Math.Ceiling(
                    total / (double)tamanoPagina)
            };
        }

        // ==========================================
        // CONTAR NO LEIDAS
        // ==========================================

        public async Task<int> ContarNoLeidasAsync(
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default)
        {
            return await context.Notificacions
                .AsNoTracking()
                .CountAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.UsuarioId == usuarioId &&
                    x.TipoDestinatario == "USUARIO" &&
                    x.MostrarInterna &&
                    x.FechaLectura == null,
                    ct);
        }

        // ==========================================
        // MARCAR UNA COMO LEIDA
        // ==========================================

        public async Task<bool> MarcarLeidaAsync(
            Guid empresaId,
            Guid usuarioId,
            Guid notificacionId,
            CancellationToken ct = default)
        {
            if (notificacionId == Guid.Empty)
            {
                return false;
            }

            // ======================================
            // VERIFICAR PROPIEDAD
            // ======================================

            var existe = await context.Notificacions
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == notificacionId &&
                    x.EmpresaId == empresaId &&
                    x.UsuarioId == usuarioId &&
                    x.TipoDestinatario == "USUARIO" &&
                    x.MostrarInterna,
                    ct);

            if (!existe)
            {
                return false;
            }

            // ======================================
            // ACTUALIZAR
            // ======================================

            var ahora = DateTime.UtcNow;

            await context.Notificacions
                .Where(x =>
                    x.Id == notificacionId &&
                    x.EmpresaId == empresaId &&
                    x.UsuarioId == usuarioId &&
                    x.TipoDestinatario == "USUARIO" &&
                    x.MostrarInterna &&
                    x.FechaLectura == null)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            x => x.FechaLectura,
                            ahora)
                        .SetProperty(
                            x => x.FechaActualizacion,
                            ahora),
                    ct);

            return true;
        }

        // ==========================================
        // MARCAR TODAS COMO LEIDAS
        // ==========================================

        public async Task<int> MarcarTodasLeidasAsync(
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default)
        {
            var ahora = DateTime.UtcNow;

            var actualizadas = await context.Notificacions
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.UsuarioId == usuarioId &&
                    x.TipoDestinatario == "USUARIO" &&
                    x.MostrarInterna &&
                    x.FechaLectura == null)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            x => x.FechaLectura,
                            ahora)
                        .SetProperty(
                            x => x.FechaActualizacion,
                            ahora),
                    ct);

            return actualizadas;
        }
    }
}
