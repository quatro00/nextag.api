using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.TiposContrato
{
    public class TipoContratoEstatusDto
    {
        [Required]
        public bool? Activo { get; set; }
    }
}
