using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Ubicaciones
{
    public class UbicacionCreateDto
    {
        [Required]
        public Guid SucursalId { get; set; }

        public Guid? TipoUbicacionId { get; set; }

        public Guid? UbicacionPadreId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Clave { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }
    }
}
