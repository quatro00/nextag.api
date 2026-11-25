namespace Nextag.Api.Models.Dto.ContratoActivos
{
    public class ContratoActivoDisponibleDto
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? NumeroInventario { get; set; }
        public string? NumeroSerie { get; set; }
        public Guid CategoriaActivoId { get; set; }
        public string CategoriaActivoNombre { get; set; } = string.Empty;
        public Guid SucursalId { get; set; }
        public string SucursalNombre { get; set; } = string.Empty;
        public Guid? UbicacionId { get; set; }
        public string? UbicacionNombre { get; set; }
        public int EstadoActivoId { get; set; }
        public string EstadoActivoNombre { get; set; } = string.Empty;
    }
}
