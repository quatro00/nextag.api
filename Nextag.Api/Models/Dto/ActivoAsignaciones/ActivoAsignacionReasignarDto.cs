using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.ActivoAsignaciones
{
    public class ActivoAsignacionReasignarDto
    {
        [Required]
        public Guid NuevoResponsableId { get; set; }

        public DateTimeOffset? FechaReasignacion { get; set; }

        [MaxLength(300)]
        public string? Motivo { get; set; }

        [MaxLength(100)]
        public string? CondicionDevolucion { get; set; }

        [MaxLength(1000)]
        public string? ObservacionesDevolucion { get; set; }

        [MaxLength(1000)]
        public string? ObservacionesAsignacion { get; set; }
    }
}
