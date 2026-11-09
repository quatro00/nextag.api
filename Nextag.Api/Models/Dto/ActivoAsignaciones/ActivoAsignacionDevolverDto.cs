using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.ActivoAsignaciones
{
    public class ActivoAsignacionDevolverDto
    {
        public DateTimeOffset? FechaDevolucion { get; set; }

        [MaxLength(300)]
        public string? MotivoDevolucion { get; set; }

        [MaxLength(100)]
        public string? CondicionDevolucion { get; set; }

        [MaxLength(1000)]
        public string? ObservacionesDevolucion { get; set; }
    }
}
