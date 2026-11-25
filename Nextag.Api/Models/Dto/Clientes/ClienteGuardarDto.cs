using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Clientes
{
    public class ClienteGuardarDto
    {
        [Required]
        [MaxLength(50)]
        public string Clave { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? RazonSocial { get; set; }

        [MaxLength(13)]
        public string? Rfc { get; set; }

        [EmailAddress]
        [MaxLength(200)]
        public string? Correo { get; set; }

        [MaxLength(30)]
        public string? Telefono { get; set; }

        [MaxLength(250)]
        public string? SitioWeb { get; set; }

        [MaxLength(300)]
        public string? Direccion { get; set; }

        [MaxLength(150)]
        public string? Colonia { get; set; }

        [MaxLength(150)]
        public string? Municipio { get; set; }

        [MaxLength(150)]
        public string? Estado { get; set; }

        [MaxLength(100)]
        public string? Pais { get; set; }

        [MaxLength(15)]
        public string? CodigoPostal { get; set; }

        [MaxLength(200)]
        public string? ContactoPrincipal { get; set; }

        [MaxLength(30)]
        public string? TelefonoContacto { get; set; }

        [EmailAddress]
        [MaxLength(200)]
        public string? CorreoContacto { get; set; }

        [MaxLength(1000)]
        public string? Observaciones { get; set; }
    }
}
