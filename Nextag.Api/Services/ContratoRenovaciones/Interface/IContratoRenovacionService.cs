using Nextag.Api.Models.Dto.ContratoRenovaciones;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.ContratoRenovaciones.Interface
{
    public interface IContratoRenovacionService
    {
        Task<ServiceResult<List<ContratoRenovacionDto>>>
            GetAllAsync(
                Guid contratoId,
                Guid empresaId,
                CancellationToken ct = default);

        Task<ServiceResult<ContratoRenovacionDto>>
            GetByIdAsync(
                Guid contratoId,
                Guid id,
                Guid empresaId,
                CancellationToken ct = default);

        Task<ServiceResult<ContratoRenovacionInformacionDto>>
            GetInformacionAsync(
                Guid contratoId,
                Guid empresaId,
                CancellationToken ct = default);

        Task<ServiceResult<ContratoRenovacionResultadoDto>>
            RenovarAsync(
                Guid contratoId,
                ContratoRenovacionGuardarDto dto,
                Guid empresaId,
                Guid usuarioId,
                CancellationToken ct = default);
    }
}
