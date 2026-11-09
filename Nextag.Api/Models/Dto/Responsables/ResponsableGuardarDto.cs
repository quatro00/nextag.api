using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Responsables
{
    public class ResponsableGuardarDto
    {
        [Required]
        [RegularExpression("^(PERSONA|AREA)$",
            ErrorMessage = "El tipo debe ser PERSONA o AREA.")]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Codigo { get; set; }

        [MaxLength(100)]
        public string? Puesto { get; set; }

        [MaxLength(150)]
        public string? Departamento { get; set; }

        public Guid? SucursalId { get; set; }

        public Guid? AreaResponsableId { get; set; }

        [MaxLength(150)]
        public string? Contacto { get; set; }

        [EmailAddress]
        [MaxLength(256)]
        public string? Correo { get; set; }

        [MaxLength(30)]
        public string? Telefono { get; set; }

        [MaxLength(500)]
        public string? Observaciones { get; set; }
    }
}
