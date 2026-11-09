using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.ActivoMovimientos
{
    public class ActivoMovimientoFiltroDto
    {
        [MaxLength(200)]
        public string? Buscar { get; set; }

        public Guid? ActivoId { get; set; }

        public int? TipoMovimientoActivoId { get; set; }

        public Guid? SucursalId { get; set; }

        public DateTime? FechaDesde { get; set; }

        public DateTime? FechaHasta { get; set; }

        public bool? EsDestinoExterno { get; set; }

        public bool? Anulado { get; set; }

        [Range(1, 1000000)]
        public int Pagina { get; set; } = 1;

        [Range(1, 100)]
        public int TamanoPagina { get; set; } = 20;
    }
}
