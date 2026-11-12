using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Tickets
{
    public class TicketSeguimientoCrearDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(10000)]
        public string Comentario { get; set; } = string.Empty;
    }
}
