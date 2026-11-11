using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Tickets
{
    public class TicketReasignarDto
    {
        [Required]
        public Guid EquipoSoporteId { get; set; }

        public Guid? UsuarioTecnicoId { get; set; }

        [Required]
        [MinLength(5)]
        [MaxLength(300)]
        public string Motivo { get; set; } = string.Empty;
    }
}
