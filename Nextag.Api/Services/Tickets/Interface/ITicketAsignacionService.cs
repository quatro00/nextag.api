using Nextag.Api.Models.Dto.Tickets;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.Tickets.Interface
{
    public interface ITicketAsignacionService
    {
        Task<ServiceResult<TicketAsignacionDto>> AsignarAsync(
            Guid ticketId,
            Guid empresaId,
            Guid usuarioId,
            TicketAsignarDto dto,
            CancellationToken ct = default);

        Task<ServiceResult<TicketAsignacionDto>> ReasignarAsync(
            Guid ticketId,
            Guid empresaId,
            Guid usuarioId,
            TicketReasignarDto dto,
            CancellationToken ct = default);

        Task<TicketAsignacionDto?> GetVigenteAsync(
            Guid ticketId,
            Guid empresaId,
            CancellationToken ct = default);

        Task<List<TicketAsignacionDto>> GetHistorialAsync(
            Guid ticketId,
            Guid empresaId,
            CancellationToken ct = default);

        Task<List<TicketListDto>> GetMisTicketsAsync(
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);

        Task<List<TicketListDto>> GetTicketsMisEquiposAsync(
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);

    }
}
