using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Marcas
{
    public class MarcaCreateDto
    {
        [Required]
        [MaxLength(50)]
        public string Clave { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }
    }
}
