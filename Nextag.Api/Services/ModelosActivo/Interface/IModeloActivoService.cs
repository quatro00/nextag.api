using Nextag.Api.Models.Dto.ModelosActivo;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.ModelosActivo.Interface
{
    public interface IModeloActivoService
    {
        Task<List<ModeloActivoListDto>> GetAllAsync(
            Guid empresaId,
            Guid? marcaId = null,
            Guid? categoriaActivoId = null,
            bool? activo = null,
            CancellationToken ct = default);

        Task<ModeloActivoDetalleDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default);

        Task<ServiceResult<ModeloActivoDetalleDto>> CreateAsync(
            ModeloActivoCreateDto dto,
            Guid empresaId,
            Guid usuarioCreacion,
            CancellationToken ct = default);

        Task<ServiceResult<ModeloActivoDetalleDto>> UpdateAsync(
            Guid id,
            ModeloActivoUpdateDto dto,
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
