using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.ActivoMovimientos
{
    public class ActivoMovimientoCrearDto
    {
        [Required]
        public Guid ActivoId { get; set; }

        [Required]
        public int TipoMovimientoActivoId { get; set; }

        // Destino interno
        public Guid? SucursalDestinoId { get; set; }
        public Guid? UbicacionDestinoId { get; set; }

        // Destino externo
        [MaxLength(200)]
        public string? DestinoExternoNombre { get; set; }

        [MaxLength(500)]
        public string? DestinoExternoDireccion { get; set; }
        public Guid? ProveedorDestinoId { get; set; }

        // Para retornos
        public Guid? MovimientoOrigenId { get; set; }

        // Estado posterior opcional
        public int? EstadoActivoDestinoId { get; set; }

        [MaxLength(300)]
        public string? Motivo { get; set; }

        [MaxLength(1000)]
        public string? Observaciones { get; set; }

        // Referencias futuras
        public string? TipoReferencia { get; set; }
        public Guid? ReferenciaId { get; set; }
    }
}
