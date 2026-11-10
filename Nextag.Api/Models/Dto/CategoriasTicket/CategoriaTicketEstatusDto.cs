using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.CategoriasTicket
{
    public class CategoriaTicketEstatusDto
    {
        [Required]
        public bool? Activo { get; set; }
    }
}
