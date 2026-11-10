using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.EquipoSoporteIntegrantes
{
    public class EquipoSoporteIntegranteEstatusDto
    {
        [Required]
        public bool? Activo { get; set; }
    }
}
