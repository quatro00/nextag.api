using System.ComponentModel.DataAnnotations;

namespace SIGED.api.Models.Dto.Administrador.Areas
{
    public class AreaUpdateDto
    {
        [Required]
        [MaxLength(30)]
        public string Clave { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }
    }
}
