using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Marcas
{
    public class MarcaEstatusDto
    {
        [Required]
        public bool? Activo { get; set; }
    }
}
