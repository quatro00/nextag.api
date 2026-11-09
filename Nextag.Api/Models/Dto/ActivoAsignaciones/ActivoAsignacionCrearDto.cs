using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.ActivoAsignaciones
{
    public class ActivoAsignacionCrearDto
    {
        [Required]
        public Guid ActivoId { get; set; }

        [Required]
        public Guid ResponsableId { get; set; }

        public DateTimeOffset? FechaAsignacion { get; set; }

        [MaxLength(300)]
        public string? MotivoAsignacion { get; set; }

        [MaxLength(1000)]
        public string? ObservacionesAsignacion { get; set; }
    }
}
