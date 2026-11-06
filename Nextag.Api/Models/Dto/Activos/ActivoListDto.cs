namespace Nextag.Api.Models.Dto.Activos;

public class ActivoListDto
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string? NumeroInventario { get; set; }
    public string? NumeroSerie { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Guid CategoriaActivoId { get; set; }
    public string CategoriaActivoNombre { get; set; } = string.Empty;
    public Guid? MarcaId { get; set; }
    public string? MarcaNombre { get; set; }
    public Guid? ModeloActivoId { get; set; }
    public string? ModeloActivoNombre { get; set; }
    public Guid SucursalId { get; set; }
    public string SucursalNombre { get; set; } = string.Empty;
    public Guid? UbicacionId { get; set; }
    public string? UbicacionNombre { get; set; }
    public int EstadoActivoId { get; set; }
    public string EstadoActivoNombre { get; set; } = string.Empty;
    public Guid? ProveedorId { get; set; }
    public string? ProveedorNombre { get; set; }
    public string TipoPropiedad { get; set; } = string.Empty;
}
