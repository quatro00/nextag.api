namespace Nextag.Api.Models.Dto.Contratos
{
    public class ContratoListadoDto
    {
        public List<ContratoListDto> Items { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int Pagina { get; set; }
        public int RegistrosPorPagina { get; set; }
        public int TotalPaginas { get; set; }
    }
}
