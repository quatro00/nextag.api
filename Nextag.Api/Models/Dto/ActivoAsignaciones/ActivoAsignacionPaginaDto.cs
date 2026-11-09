namespace Nextag.Api.Models.Dto.ActivoAsignaciones
{
    public class ActivoAsignacionPaginaDto
    {
        public List<ActivoAsignacionDto> Items { get; set; } = new();

        public int Total { get; set; }

        public int Pagina { get; set; }

        public int TamanoPagina { get; set; }

        public int TotalPaginas { get; set; }
    }
}
