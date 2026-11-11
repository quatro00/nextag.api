using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Tickets
{
    public class TicketReabrirDto
    {
        [Required]
        [MinLength(5)]
        [MaxLength(500)]
        public string Motivo { get; set; } = string.Empty;
    }
}
