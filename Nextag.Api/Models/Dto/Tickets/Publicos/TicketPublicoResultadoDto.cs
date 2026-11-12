namespace Nextag.Api.Models.Dto.Tickets.Publicos
{
    public class TicketPublicoResultadoDto
    {
        public Guid Id { get; set; }

        public string Folio { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;

        public DateTime FechaApertura { get; set; }
    }

    public class CategoriaTicketPublicaDto
    {
        public Guid Id { get; set; }

        public string Nombre { get; set; } = string.Empty;
    }
}
