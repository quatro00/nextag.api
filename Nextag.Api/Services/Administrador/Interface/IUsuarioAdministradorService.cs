using Nextag.Api.Models.Dto.Administrador.Usuarios;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.Administrador.Interface
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
