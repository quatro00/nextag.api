using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.ModelosActivo
{
    public class ModeloActivoEstatusDto
    {
        [Required]
        public bool? Activo { get; set; }
    }
}
