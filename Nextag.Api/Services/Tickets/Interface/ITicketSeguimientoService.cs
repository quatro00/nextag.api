using Nextag.Api.Models.Dto.Tickets;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.Tickets.Interface
{
    public interface ITicketSeguimientoService
    {
        Task<ServiceResult<List<TicketSeguimientoDto>>>
            GetAllAsync(
                Guid ticketId,
                Guid empresaId,
                Guid usuarioId,
                bool esAdministrador,
                CancellationToken ct = default);

        Task<ServiceResult<TicketSeguimientoDto>>
            GetByIdAsync(
                Guid ticketId,
                Guid seguimientoId,
                Guid empresaId,
                Guid usuarioId,
                bool esAdministrador,
                CancellationToken ct = default);

        Task<ServiceResult<TicketSeguimientoDto>>
            AgregarComentarioAsync(
                Guid ticketId,
                Guid empresaId,
                Guid usuarioId,
                bool esAdministrador,
                TicketSeguimientoCrearDto dto,
                CancellationToken ct = default);

        Task<ServiceResult<TicketSeguimientoDto>>
            AgregarNotaInternaAsync(
                Guid ticketId,
                Guid empresaId,
                Guid usuarioId,
                bool esAdministrador,
                TicketSeguimientoCrearDto dto,
                CancellationToken ct = default);
    }
}
