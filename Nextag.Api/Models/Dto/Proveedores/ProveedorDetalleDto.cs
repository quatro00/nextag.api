namespace Nextag.Api.Models.Dto.Proveedores;

public class ProveedorDetalleDto : ProveedorListDto
{
    public string? SitioWeb { get; set; }
    public string? Direccion { get; set; }
    public string? Colonia { get; set; }
    public string? Municipio { get; set; }
    public string? Estado { get; set; }
    public string? Pais { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Observaciones { get; set; }
    public DateTime FechaCreacion { get; set; }
    public Guid UsuarioCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public Guid? UsuarioModificacion { get; set; }
}
