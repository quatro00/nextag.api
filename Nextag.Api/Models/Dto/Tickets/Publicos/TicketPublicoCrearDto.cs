using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Tickets.Publicos
{
    public class TicketPublicoCrearDto
    {
        [Required]
        [StringLength(150, MinimumLength = 3)]
        public string Nombre { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(254)]
        public string? Correo { get; set; }

        [MaxLength(30)]
        public string? Telefono { get; set; }

        public Guid? CategoriaTicketId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [StringLength(5000, MinimumLength = 10)]
        public string Descripcion { get; set; } = string.Empty;
    }
}
