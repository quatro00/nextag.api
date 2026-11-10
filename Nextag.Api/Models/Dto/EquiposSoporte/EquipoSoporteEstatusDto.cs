using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.EquiposSoporte
{
    public class EquipoSoporteEstatusDto
    {
        [Required]
        public bool? Activo { get; set; }
    }
}
