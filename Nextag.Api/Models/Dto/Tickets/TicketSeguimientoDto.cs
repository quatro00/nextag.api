namespace Nextag.Api.Models.Dto.Tickets
{
    public class TicketSeguimientoDto
    {
        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public Guid TicketId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Comentario { get; set; } = string.Empty;
        public bool VisibleSolicitante { get; set; }
        public Guid? AutorUsuarioId { get; set; }
        public string? AutorNombre { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
