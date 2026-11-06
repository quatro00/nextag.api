using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Proveedores;

// Se reutiliza para POST y PUT; EmpresaId y auditoría provienen del JWT/contexto.
public class ProveedorGuardarDto
{
    [Required, MaxLength(50)]
    public string Clave { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(250)] public string? RazonSocial { get; set; }
    [MaxLength(13)] public string? Rfc { get; set; }
    [EmailAddress, MaxLength(256)] public string? Correo { get; set; }
    [MaxLength(30)] public string? Telefono { get; set; }
    [MaxLength(300)] public string? SitioWeb { get; set; }
    [MaxLength(300)] public string? Direccion { get; set; }
    [MaxLength(150)] public string? Colonia { get; set; }
    [MaxLength(150)] public string? Municipio { get; set; }
    [MaxLength(150)] public string? Estado { get; set; }
    [MaxLength(100)] public string? Pais { get; set; }
    [MaxLength(15)] public string? CodigoPostal { get; set; }
    [MaxLength(1000)] public string? Observaciones { get; set; }

    // Al guardar se reemplaza el conjunto completo de tipos asignados.
    // Se requiere al menos un tipo activo.
    public List<int> TipoProveedorIds { get; set; } = new();
}
