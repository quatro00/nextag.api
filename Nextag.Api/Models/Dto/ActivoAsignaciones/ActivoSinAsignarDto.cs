namespace Nextag.Api.Models.Dto.ActivoAsignaciones
{
    public class ActivoSinAsignarDto
    {
        public Guid Id { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? NumeroSerie { get; set; }

        public string? NumeroInventario { get; set; }

        public Guid SucursalId { get; set; }

        public string SucursalNombre { get; set; } = string.Empty;

        public Guid? UbicacionId { get; set; }

        public string? UbicacionNombre { get; set; }

        public int EstadoActivoId { get; set; }

        public string EstadoActivoNombre { get; set; } = string.Empty;
    }

    public class ActivosSinAsignarPaginaDto
    {
        public List<ActivoSinAsignarDto> Items { get; set; } = new();

        public int Total { get; set; }

        public int Pagina { get; set; }

        public int TamanoPagina { get; set; }

        public int TotalPaginas { get; set; }
    }
}
