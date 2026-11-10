namespace Nextag.Api.Models.Dto.Tickets
{
    public class TicketPaginadoDto
    {
        public List<TicketListDto> Items { get; set; } = new();

        public int TotalRegistros { get; set; }

        public int Pagina { get; set; }

        public int TamanoPagina { get; set; }

        public int TotalPaginas { get; set; }
    }
}
