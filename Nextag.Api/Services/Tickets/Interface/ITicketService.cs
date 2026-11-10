using Nextag.Api.Models.Dto.Tickets;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.Tickets.Interface
{
    public interface ITicketService
    {
        Task<TicketPaginadoDto> GetAllAsync(
            Guid empresaId,
            TicketFiltroDto filtro,
            CancellationToken ct = default);

        Task<TicketDetalleDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default);

        Task<List<TicketListDto>> GetByActivoAsync(
            Guid activoId,
            Guid empresaId,
            CancellationToken ct = default);

        Task<List<TicketCatalogoDto>> GetEstadosAsync(
            CancellationToken ct = default);

        Task<List<TicketCatalogoDto>> GetPrioridadesAsync(
            CancellationToken ct = default);

        Task<ServiceResult<TicketDetalleDto>> CreateAsync(
            TicketCrearDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);

        Task<ServiceResult<TicketDetalleDto>> UpdateAsync(
            Guid id,
            TicketActualizarDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);
    }
}
