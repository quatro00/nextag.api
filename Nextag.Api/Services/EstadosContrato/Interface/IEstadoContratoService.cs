using Nextag.Api.Models.Dto.EstadosContrato;

namespace Nextag.Api.Services.EstadosContrato.Interface
{
    public interface IEstadoContratoService
    {
        Task<List<EstadoContratoDto>> GetAllAsync(
            bool? activo = null,
            CancellationToken ct = default);
    }
}
