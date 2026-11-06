using Nextag.Api.Models.Dto.TiposProveedor;

namespace Nextag.Api.Services.TiposProveedor.Interface;

public interface ITipoProveedorService
{
    Task<List<TipoProveedorDto>> GetAllAsync(CancellationToken ct = default);
}
