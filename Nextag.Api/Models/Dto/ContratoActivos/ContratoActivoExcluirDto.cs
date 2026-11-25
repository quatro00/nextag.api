using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.ContratoActivos
{
    public class ContratoActivoExcluirDto
    {
        [Required]
        [MaxLength(500)]
        public string Motivo { get; set; } = string.Empty;
    }
}
