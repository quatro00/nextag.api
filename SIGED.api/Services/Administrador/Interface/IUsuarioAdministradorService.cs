using SIGED.api.Models.Dto.Administrador.Usuarios;
using SIGED.api.Models.Dto;

namespace SIGED.api.Services.Administrador.Interface
{
    public interface IUsuarioAdministradorService
    {
        Task<List<UsuarioListDto>> GetAllAsync();
        Task<UsuarioDetalleDto?> GetByIdAsync(Guid id);
        Task<ServiceResult<UsuarioDetalleDto>> CreateAsync(UsuarioCreateDto dto);
        Task<ServiceResult<UsuarioDetalleDto>> UpdateAsync(Guid id, UsuarioUpdateDto dto);
        Task<ServiceResult<bool>> UpdateStatusAsync(Guid id, bool activo);
        Task<ServiceResult<UsuarioDetalleDto>> UpdateRolesAsync(Guid id, List<string> roles);
    }
}
