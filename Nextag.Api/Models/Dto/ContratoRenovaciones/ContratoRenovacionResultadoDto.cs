using Nextag.Api.Models.Dto.Contratos;

namespace Nextag.Api.Models.Dto.ContratoRenovaciones
{
    public class ContratoRenovacionResultadoDto
    {
        public ContratoRenovacionDto Renovacion { get; set; } = new();
        public ContratoDetalleDto Contrato { get; set; } = new();
    }
}
