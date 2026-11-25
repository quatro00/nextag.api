using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Contratos
{
    public class ContratoCancelarDto
    {
        [Required]
        [MaxLength(500)]
        public string Motivo { get; set; } = string.Empty;
    }
}
