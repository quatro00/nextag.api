using Nextag.Api.Models.Dto.CategoriasTicket;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.CategoriasTicket.Interface
{
    public interface ICategoriaTicketService
    {
        Task<List<CategoriaTicketListDto>> GetAllAsync(
            Guid empresaId,
            bool? activo = null,
            string? buscar = null,
            CancellationToken ct = default);

        Task<CategoriaTicketDetalleDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default);

        Task<ServiceResult<CategoriaTicketDetalleDto>> CreateAsync(
            CategoriaTicketGuardarDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);

        Task<ServiceResult<CategoriaTicketDetalleDto>> UpdateAsync(
            Guid id,
            CategoriaTicketGuardarDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);

        Task<ServiceResult<bool>> UpdateStatusAsync(
            Guid id,
            Guid empresaId,
            bool activo,
            Guid usuarioId,
            CancellationToken ct = default);
    }
}
