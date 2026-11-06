using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Proveedores;

public class ProveedorContactoGuardarDto
{
    [Required, MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(150)] public string? Cargo { get; set; }
    [EmailAddress, MaxLength(256)] public string? Correo { get; set; }
    [MaxLength(30)] public string? Telefono { get; set; }
    [MaxLength(30)] public string? Celular { get; set; }
    public bool EsPrincipal { get; set; }
}
