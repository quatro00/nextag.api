namespace Nextag.Api.Models.Dto.Activos;

public class ActivoDetalleDto : ActivoListDto
{
    public string? Descripcion { get; set; }
    public Guid? PropietarioProveedorId { get; set; }
    public string? PropietarioProveedorNombre { get; set; }
    public string? PropietarioNombre { get; set; }
    public DateTime? FechaAdquisicion { get; set; }
    public decimal? CostoAdquisicion { get; set; }
    public string Moneda { get; set; } = "MXN";
    public DateTime FechaAlta { get; set; }
    public DateTime? FechaBaja { get; set; }
    public string? Observaciones { get; set; }
    public DateTime FechaCreacion { get; set; }
    public Guid UsuarioCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public Guid? UsuarioModificacion { get; set; }
}
