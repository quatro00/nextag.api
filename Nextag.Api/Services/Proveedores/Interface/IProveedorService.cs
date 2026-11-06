using Nextag.Api.Models.Dto;
using Nextag.Api.Models.Dto.Proveedores;

namespace Nextag.Api.Services.Proveedores.Interface;

public interface IProveedorService
{
    Task<List<ProveedorListDto>> GetAllAsync(
        Guid empresaId, bool? activo = null, int? tipoProveedorId = null,
        string? buscar = null, CancellationToken ct = default);

    Task<ProveedorDetalleDto?> GetByIdAsync(
        Guid id, Guid empresaId, CancellationToken ct = default);

    Task<ServiceResult<ProveedorDetalleDto>> CreateAsync(
        ProveedorGuardarDto dto, Guid empresaId, Guid usuarioId,
        CancellationToken ct = default);

    Task<ServiceResult<ProveedorDetalleDto>> UpdateAsync(
        Guid id, ProveedorGuardarDto dto, Guid empresaId, Guid usuarioId,
        CancellationToken ct = default);

    Task<ServiceResult<bool>> UpdateStatusAsync(
        Guid id, Guid empresaId, bool activo, Guid usuarioId,
        CancellationToken ct = default);
}
