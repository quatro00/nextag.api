namespace Nextag.Api.Models.Dto.ContratoActivos
{
    public class ContratoActivoDto
    {
        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public Guid ContratoId { get; set; }
        public Guid ActivoId { get; set; }
        public string ActivoCodigo { get; set; } = string.Empty;
        public string ActivoNombre { get; set; } = string.Empty;
        public string? NumeroInventario { get; set; }
        public string? NumeroSerie { get; set; }
        public Guid CategoriaActivoId { get; set; }
        public string CategoriaActivoNombre { get; set; } = string.Empty;
        public int EstadoActivoId { get; set; }
        public string EstadoActivoNombre { get; set; } = string.Empty;
        public DateTime FechaInclusion { get; set; }
        public DateTime? FechaExclusion { get; set; }
        public string? MotivoExclusion { get; set; }
        public string? Observaciones { get; set; }
        public bool AsociacionVigente { get; set; }
        public DateTime FechaCreacion { get; set; }
        public Guid UsuarioCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public Guid? UsuarioModificacion { get; set; }
    }
}
