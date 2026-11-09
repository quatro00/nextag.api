namespace Nextag.Api.Models.Dto.ActivoMovimientos
{
    public class ActivoMovimientoPaginaDto
    {
        public List<ActivoMovimientoDto> Items { get; set; } = new();

        public int Total { get; set; }

        public int Pagina { get; set; }

        public int TamanoPagina { get; set; }

        public int TotalPaginas { get; set; }
    }
}
