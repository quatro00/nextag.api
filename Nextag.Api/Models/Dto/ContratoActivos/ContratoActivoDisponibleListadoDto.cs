namespace Nextag.Api.Models.Dto.ContratoActivos
{
    public class ContratoActivoDisponibleListadoDto
    {
        public List<ContratoActivoDisponibleDto> Items { get; set; }
            = new();

        public int TotalRegistros { get; set; }
        public int Pagina { get; set; }
        public int RegistrosPorPagina { get; set; }
        public int TotalPaginas { get; set; }
    }
}
