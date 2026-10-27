using Nextag.Api.Models.Dto.Administrador.Areas;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.Administrador.Interface
{
    public interface IAreaAdministradorService
    {
        Task<List<AreaListDto>> GetAllAsync();
        Task<AreaDetalleDto?> GetByIdAsync(Guid id);
        Task<ServiceResult<AreaDetalleDto>> CreateAsync(AreaCreateDto dto);
        Task<ServiceResult<AreaDetalleDto>> UpdateAsync(Guid id, AreaUpdateDto dto);
        Task<ServiceResult<bool>> UpdateStatusAsync(Guid id, bool activo);
    }
}
