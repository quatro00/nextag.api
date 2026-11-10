using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.EquiposSoporte
{
    public class EquipoSoporteGuardarDto
    {
        [Required]
        [MaxLength(40)]
        public string Clave { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        [MaxLength(15)]
        public string Tipo { get; set; } = "INTERNO";

        public Guid? ProveedorId { get; set; }

        [EmailAddress]
        [MaxLength(200)]
        public string? CorreoContacto { get; set; }
    }
}
