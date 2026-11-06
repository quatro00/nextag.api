namespace Nextag.Api.Models.Dto.Proveedores;

public class ProveedorListDto
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? RazonSocial { get; set; }
    public string? Rfc { get; set; }
    public string? Correo { get; set; }
    public string? Telefono { get; set; }
    public bool Activo { get; set; }
    public List<int> TipoProveedorIds { get; set; } = new();
    public List<string> TiposProveedor { get; set; } = new();
}
