using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.ActivoAsignaciones
{
    public class ActivoAsignacionFiltroDto
    {
        [MaxLength(200)]
        public string? Buscar { get; set; }

        public Guid? ActivoId { get; set; }

        public Guid? ResponsableId { get; set; }

        public Guid? SucursalId { get; set; }

        public string? TipoResponsable { get; set; }

        public bool? Vigente { get; set; }

        public DateTime? FechaDesde { get; set; }

        public DateTime? FechaHasta { get; set; }

        [Range(1, 1000000)]
        public int Pagina { get; set; } = 1;

        [Range(1, 100)]
        public int TamanoPagina { get; set; } = 20;
    }
}
