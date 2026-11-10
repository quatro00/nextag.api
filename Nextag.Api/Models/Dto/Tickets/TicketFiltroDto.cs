using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Tickets
{
    public class TicketFiltroDto
    {
        public string? Buscar { get; set; }

        public int? EstadoTicketId { get; set; }

        public int? PrioridadTicketId { get; set; }

        public Guid? CategoriaTicketId { get; set; }

        public Guid? ActivoId { get; set; }

        public DateTime? FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        [Range(1, int.MaxValue)]
        public int Pagina { get; set; } = 1;

        [Range(1, 100)]
        public int TamanoPagina { get; set; } = 20;
    }
}
