using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.ModelosActivo
{
    public class ModeloActivoCreateDto
    {
        [Required]
        public Guid MarcaId { get; set; }

        [Required]
        public Guid CategoriaActivoId { get; set; }

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
