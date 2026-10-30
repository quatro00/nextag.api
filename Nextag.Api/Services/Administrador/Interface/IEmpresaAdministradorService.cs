using Nextag.Api.Models.Dto.Administrador.Empresas;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.Administrador.Interface
{
    public interface IEmpresaAdministradorService
    {
        Task<List<EmpresaListDto>> GetAllAsync();

        Task<EmpresaDetalleDto?> GetByIdAsync(Guid id);

        Task<ServiceResult<EmpresaDetalleDto>> CreateAsync(EmpresaCreateDto dto, Guid usuarioId);

        Task<ServiceResult<EmpresaDetalleDto>> UpdateAsync(Guid id, EmpresaUpdateDto dto, Guid usuarioId);

        Task<ServiceResult<bool>> UpdateStatusAsync(Guid id, bool activo, Guid usuarioId);
    }
}
