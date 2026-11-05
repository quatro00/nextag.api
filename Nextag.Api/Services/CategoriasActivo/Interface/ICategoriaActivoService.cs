using Nextag.Api.Models.Dto.CategoriasActivo;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.CategoriasActivo.Interface
{
    public interface ICategoriaActivoService
    {
        Task<List<CategoriaActivoListDto>> GetAllAsync(
            Guid empresaId,
            bool? activo = null,
            CancellationToken ct = default);

        Task<CategoriaActivoDetalleDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default);

        Task<ServiceResult<CategoriaActivoDetalleDto>> CreateAsync(
            CategoriaActivoCreateDto dto,
            Guid empresaId,
            Guid usuarioCreacion,
            CancellationToken ct = default);

        Task<ServiceResult<CategoriaActivoDetalleDto>> UpdateAsync(
            Guid id,
            CategoriaActivoUpdateDto dto,
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
