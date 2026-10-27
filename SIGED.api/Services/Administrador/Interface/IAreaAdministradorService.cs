using SIGED.api.Models.Dto.Administrador.Areas;
using SIGED.api.Models.Dto;

namespace SIGED.api.Services.Administrador.Interface
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
