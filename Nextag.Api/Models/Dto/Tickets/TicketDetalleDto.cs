namespace Nextag.Api.Models.Dto.Tickets
{
    public class TicketDetalleDto : TicketListDto
    {
        public string Descripcion { get; set; } = string.Empty;

        public string? SolicitanteCorreo { get; set; }

        public string? SolicitanteTelefono { get; set; }

        public DateTime? FechaPrimeraAtencion { get; set; }

        public DateTime? FechaResolucion { get; set; }

        public DateTime? FechaCierre { get; set; }

        public DateTime? FechaCancelacion { get; set; }

        public string? MotivoCancelacion { get; set; }

        public DateTime FechaCreacion { get; set; }

        public Guid? UsuarioCreacion { get; set; }

        public DateTime? FechaActualizacion { get; set; }

        public Guid? UsuarioModificacion { get; set; }
    }
}
