namespace Nextag.Api.Models.Dto.Tickets
{
    public class TicketListDto
    {
        public Guid Id { get; set; }

        public Guid EmpresaId { get; set; }

        public long Numero { get; set; }

        public string Folio { get; set; } = string.Empty;

        public Guid? ActivoId { get; set; }

        public string? ActivoCodigo { get; set; }

        public string? ActivoNombre { get; set; }

        public Guid? CategoriaTicketId { get; set; }

        public string? CategoriaNombre { get; set; }

        public int PrioridadTicketId { get; set; }

        public string PrioridadNombre { get; set; } = string.Empty;

        public int EstadoTicketId { get; set; }

        public string EstadoNombre { get; set; } = string.Empty;

        public string Origen { get; set; } = string.Empty;

        public string Titulo { get; set; } = string.Empty;

        public Guid? SolicitanteUsuarioId { get; set; }

        public string? SolicitanteNombre { get; set; }

        public DateTime FechaApertura { get; set; }
    }
}
