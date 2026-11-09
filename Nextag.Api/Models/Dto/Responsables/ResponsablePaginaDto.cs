namespace Nextag.Api.Models.Dto.Responsables
{
    public class ResponsablePaginaDto
    {
        public List<ResponsableDto> Items { get; set; } = new();

        public int Total { get; set; }

        public int Pagina { get; set; }

        public int TamanoPagina { get; set; }

        public int TotalPaginas { get; set; }
    }
}
