using Nextag.Api.Models.Dto.Tickets;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.Tickets.Interface
{
    public interface ITicketEstadoService
    {
        Task<List<TicketEstadoHistorialDto>> GetHistorialAsync(
        Guid ticketId,
        Guid empresaId,
        CancellationToken ct = default);

        Task<ServiceResult<TicketDetalleDto>> IniciarAsync(
            Guid ticketId,
            Guid empresaId,
            Guid usuarioId,
            bool esAdministrador,
            CancellationToken ct = default);

        Task<ServiceResult<TicketDetalleDto>> ResolverAsync(
            Guid ticketId,
            Guid empresaId,
            Guid usuarioId,
            bool esAdministrador,
            TicketResolverDto dto,
            CancellationToken ct = default);

        Task<ServiceResult<TicketDetalleDto>> CerrarAsync(
            Guid ticketId,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);

        Task<ServiceResult<TicketDetalleDto>> ReabrirAsync(
            Guid ticketId,
            Guid empresaId,
            Guid usuarioId,
            TicketReabrirDto dto,
            CancellationToken ct = default);

        Task<ServiceResult<TicketDetalleDto>> CancelarAsync(
            Guid ticketId,
            Guid empresaId,
            Guid usuarioId,
            TicketCancelarDto dto,
            CancellationToken ct = default);
    }
}
