using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.ContratoActivos
{
    public class ContratoActivoAsociarDto
    {
        [Required]
        [MinLength(1)]
        [MaxLength(200)]
        public List<Guid> ActivoIds { get; set; } = new();

        [MaxLength(1000)]
        public string? Observaciones { get; set; }
    }
}
