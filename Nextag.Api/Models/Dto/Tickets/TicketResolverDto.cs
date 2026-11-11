using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Tickets
{
    public class TicketResolverDto
    {
        [Required]
        [MinLength(5)]
        public string Solucion { get; set; } = string.Empty;
    }
}
