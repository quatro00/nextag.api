namespace Nextag.Api.Models.Dto.Tickets
{
    public class TicketEstadoHistorialDto
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public int? EstadoAnteriorId { get; set; }
        public string? EstadoAnteriorNombre { get; set; }
        public int EstadoNuevoId { get; set; }
        public string EstadoNuevoNombre { get; set; } = string.Empty;
        public string? Motivo { get; set; }
        public DateTime FechaCambio { get; set; }
        public Guid? UsuarioId { get; set; }
    }
}
