using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Tickets
{
    public class TicketAsignarDto
    {
        [Required]
        public Guid EquipoSoporteId { get; set; }

        public Guid? UsuarioTecnicoId { get; set; }

        [MaxLength(300)]
        public string? Motivo { get; set; }
    }
}
