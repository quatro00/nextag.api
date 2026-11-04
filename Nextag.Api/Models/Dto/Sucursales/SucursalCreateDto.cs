using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Sucursales
{
    public class SucursalCreateDto
    {
        [Required]
        [MaxLength(50)]
        public string Clave { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [MaxLength(300)]
        public string? Direccion { get; set; }

        [MaxLength(150)]
        public string? Colonia { get; set; }

        [MaxLength(150)]
        public string? Municipio { get; set; }

        [MaxLength(150)]
        public string? Estado { get; set; }

        [MaxLength(15)]
        public string? CodigoPostal { get; set; }

        [MaxLength(100)]
        public string? Pais { get; set; }

        [MaxLength(30)]
        public string? Telefono { get; set; }

        [EmailAddress]
        [MaxLength(256)]
        public string? Correo { get; set; }
    }
}
