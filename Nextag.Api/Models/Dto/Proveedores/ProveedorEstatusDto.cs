using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Proveedores;

public class ProveedorEstatusDto
{
    [Required]
    public bool? Activo { get; set; }
}
