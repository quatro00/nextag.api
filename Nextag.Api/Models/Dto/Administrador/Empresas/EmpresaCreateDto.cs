using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Administrador.Empresas
{
    public class EmpresaCreateDto
    {
        [Required]
        [MaxLength(50)]
        public string Clave { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? RazonSocial { get; set; }

        [MaxLength(20)]
        public string? Rfc { get; set; }

        [EmailAddress]
        [MaxLength(256)]
        public string? Correo { get; set; }

        [MaxLength(30)]
        public string? Telefono { get; set; }

        [MaxLength(250)]
        public string? SitioWeb { get; set; }

        [MaxLength(500)]
        public string? LogoUrl { get; set; }
    }
}
