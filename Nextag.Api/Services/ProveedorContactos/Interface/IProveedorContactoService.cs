using Nextag.Api.Models.Dto;
using Nextag.Api.Models.Dto.Proveedores;

namespace Nextag.Api.Services.ProveedorContactos.Interface;

public interface IProveedorContactoService
{
    // null significa que el proveedor no existe en la empresa.
    Task<List<ProveedorContactoDto>?> GetAllAsync(
        Guid proveedorId, Guid empresaId, bool? activo = null,
        CancellationToken ct = default);

    Task<ProveedorContactoDto?> GetByIdAsync(
        Guid proveedorId, Guid contactoId, Guid empresaId,
        CancellationToken ct = default);

    Task<ServiceResult<ProveedorContactoDto>> CreateAsync(
        Guid proveedorId, ProveedorContactoGuardarDto dto,
        Guid empresaId, Guid usuarioId, CancellationToken ct = default);

    Task<ServiceResult<ProveedorContactoDto>> UpdateAsync(
        Guid proveedorId, Guid contactoId, ProveedorContactoGuardarDto dto,
        Guid empresaId, Guid usuarioId, CancellationToken ct = default);

    Task<ServiceResult<bool>> UpdateStatusAsync(
        Guid proveedorId, Guid contactoId, Guid empresaId,
        bool activo, Guid usuarioId, CancellationToken ct = default);
}
