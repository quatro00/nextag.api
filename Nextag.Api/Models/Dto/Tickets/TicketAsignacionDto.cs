namespace Nextag.Api.Models.Dto.Tickets
{
    public class TicketAsignacionDto
    {
        public Guid Id { get; set; }

        public Guid EmpresaId { get; set; }

        public Guid TicketId { get; set; }

        public string TicketFolio { get; set; } = string.Empty;

        public string TicketTitulo { get; set; } = string.Empty;

        public Guid EquipoSoporteId { get; set; }

        public string EquipoSoporteNombre { get; set; } = string.Empty;

        public string EquipoSoporteTipo { get; set; } = string.Empty;

        public Guid? UsuarioTecnicoId { get; set; }

        public string? UsuarioTecnicoNombre { get; set; }

        public string? UsuarioTecnicoCorreo { get; set; }

        public DateTime FechaAsignacion { get; set; }

        public DateTime? FechaFin { get; set; }

        public string? Motivo { get; set; }

        public Guid UsuarioCreacion { get; set; }

        public bool Vigente { get; set; }
    }
}
