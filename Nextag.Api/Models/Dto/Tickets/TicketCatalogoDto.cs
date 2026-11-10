namespace Nextag.Api.Models.Dto.Tickets
{
    public class TicketCatalogoDto
    {
        public int Id { get; set; }

        public string Clave { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public int Orden { get; set; }
    }
}
