using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Activos;

public class ActivoFiltroDto
{
    public int? EstadoActivoId { get; set; }
    public Guid? CategoriaActivoId { get; set; }
    public Guid? MarcaId { get; set; }
    public Guid? ModeloActivoId { get; set; }
    public Guid? SucursalId { get; set; }
    public Guid? UbicacionId { get; set; }
    public Guid? ProveedorId { get; set; }
    [MaxLength(200)] public string? Buscar { get; set; }

    [Range(1, 1000000)] public int Pagina { get; set; } = 1;
    [Range(1, 100)] public int TamanoPagina { get; set; } = 20;
}
