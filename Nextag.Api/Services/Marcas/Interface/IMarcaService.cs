using Nextag.Api.Models.Dto.Marcas;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.Marcas.Interface
{
    public interface IMarcaService
    {
        Task<List<MarcaListDto>> GetAllAsync(
            Guid empresaId,
            bool? activo = null,
            CancellationToken ct = default);

        Task<MarcaDetalleDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default);

        Task<ServiceResult<MarcaDetalleDto>> CreateAsync(
            MarcaCreateDto dto,
            Guid empresaId,
            Guid usuarioCreacion,
            CancellationToken ct = default);

        Task<ServiceResult<MarcaDetalleDto>> UpdateAsync(
            Guid id,
            MarcaUpdateDto dto,
            Guid empresaId,
            Guid usuarioModificacion,
            CancellationToken ct = default);

        Task<ServiceResult<bool>> UpdateStatusAsync(
            Guid id,
            Guid empresaId,
            bool activo,
            Guid usuarioModificacion,
            CancellationToken ct = default);
    }
}
