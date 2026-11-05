using Nextag.Api.Models.Dto.EstadosActivo;

namespace Nextag.Api.Services.EstadosActivo.Interface
{
    public interface IEstadoActivoService
    {
        Task<List<EstadoActivoDto>> GetAllAsync(
            CancellationToken ct = default);

        Task<EstadoActivoDto?> GetByIdAsync(
            int id,
            CancellationToken ct = default);
    }
}
