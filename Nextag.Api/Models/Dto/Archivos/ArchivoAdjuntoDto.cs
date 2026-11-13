namespace Nextag.Api.Models.Dto.Archivos
{
    public class ArchivoAdjuntoDto
    {
        public Guid Id { get; set; }

        public Guid EmpresaId { get; set; }

        public Guid? TicketId { get; set; }

        public Guid? TicketSeguimientoId { get; set; }

        public string NombreOriginal { get; set; } =
            string.Empty;

        public string MimeType { get; set; } =
            string.Empty;

        public long TamanoBytes { get; set; }

        public string EstadoAlmacenamiento { get; set; } =
            string.Empty;

        public bool VisibleSolicitante { get; set; }

        public DateTime FechaCreacion { get; set; }

        public Guid? UsuarioCreacion { get; set; }
    }
}
