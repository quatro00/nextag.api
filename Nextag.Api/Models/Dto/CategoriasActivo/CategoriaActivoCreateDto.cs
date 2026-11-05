using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.CategoriasActivo
{
    public class CategoriaActivoCreateDto
    {
        public Guid? CategoriaPadreId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Clave { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }
    }
}
