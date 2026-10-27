using SIGED.api.Models.Dto.Administrador.Departamentos;
using SIGED.api.Models.Dto;

namespace SIGED.api.Services.Administrador.Interface
{
    public interface IDepartamentoAdministradorService
    {
        Task<List<DepartamentoListDto>> GetAllAsync(Guid? areaId = null);
        Task<DepartamentoDetalleDto?> GetByIdAsync(Guid id);
        Task<ServiceResult<DepartamentoDetalleDto>> CreateAsync(DepartamentoCreateDto dto, Guid usuarioId);
        Task<ServiceResult<DepartamentoDetalleDto>> UpdateAsync(Guid id, DepartamentoUpdateDto dto, Guid usuarioId);
        Task<ServiceResult<bool>> UpdateStatusAsync(Guid id, bool activo, Guid usuarioId);
    }
}
