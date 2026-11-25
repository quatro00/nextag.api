using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Contratos
{
    public class ContratoFiltroDto
    {
        public string? Buscar { get; set; }
        public Guid? TipoContratoId { get; set; }
        public int? EstadoContratoId { get; set; }
        public Guid? ClienteId { get; set; }
        public Guid? ProveedorId { get; set; }
        public bool? Activo { get; set; } = true;
        public bool? RenovacionAutomatica { get; set; }
        public string? Moneda { get; set; }
        public DateTime? FechaInicioDesde { get; set; }
        public DateTime? FechaInicioHasta { get; set; }
        public DateTime? FechaFinDesde { get; set; }
        public DateTime? FechaFinHasta { get; set; }
        public bool? Vencidos { get; set; }
        public bool? ProximosAVencer { get; set; }
        public bool? SinFechaFin { get; set; }

        [Range(1, 365)]
        public int DiasProximosAVencer { get; set; } = 30;

        [Range(1, int.MaxValue)]
        public int Pagina { get; set; } = 1;

        [Range(1, 200)]
        public int RegistrosPorPagina { get; set; } = 25;
    }
}
