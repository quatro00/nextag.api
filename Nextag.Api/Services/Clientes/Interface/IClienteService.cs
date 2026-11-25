using Nextag.Api.Models.Dto.Clientes;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.Clientes.Interface
{
    public interface IClienteService
    {
        Task<List<ClienteListDto>> GetAllAsync(
            Guid empresaId,
            bool? activo = null,
            string? buscar = null,
            CancellationToken ct = default);

        Task<ClienteDetalleDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default);

        Task<ServiceResult<ClienteDetalleDto>> CreateAsync(
            ClienteGuardarDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);

        Task<ServiceResult<ClienteDetalleDto>> UpdateAsync(
            Guid id,
            ClienteGuardarDto dto,
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
