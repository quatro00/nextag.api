using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Tickets
{
    public class TicketCrearDto
    {
        public Guid? ActivoId { get; set; }

        public Guid? CategoriaTicketId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int PrioridadTicketId { get; set; } = 2;

        [Required]
        [MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string Descripcion { get; set; } = string.Empty;
    }
}
