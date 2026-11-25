using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Contratos
{
    public class ContratoGuardarDto
    {
        [Required]
        [MaxLength(50)]
        public string NumeroContrato { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public Guid TipoContratoId { get; set; }
        public Guid? ClienteId { get; set; }
        public Guid? ProveedorId { get; set; }

        [Required]
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public DateTime? FechaFirma { get; set; }
        public bool RenovacionAutomatica { get; set; }

        [Range(0, int.MaxValue)]
        public int? DiasAvisoNoRenovacion { get; set; }

        [Range(typeof(decimal), "0", "9999999999999999.99")]
        public decimal? Monto { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3)]
        [RegularExpression("^[A-Za-z]{3}$")]
        public string Moneda { get; set; } = "MXN";

        [MaxLength(20)]
        public string? PeriodicidadPago { get; set; }

        [Range(1, 31)]
        public int? DiaPago { get; set; }
        public bool IncluyeImpuestos { get; set; }
        public bool? IncluyePreventivo { get; set; }
        public bool? IncluyeCorrectivo { get; set; }
        public bool? IncluyeRefacciones { get; set; }
        public bool? IncluyeManoObra { get; set; }

        [Range(typeof(decimal), "0", "99999999.99")]
        public decimal? TiempoRespuestaHoras { get; set; }

        [MaxLength(200)]
        public string? HorarioAtencion { get; set; }

        [Range(0, int.MaxValue)]
        public int? ServiciosIncluidos { get; set; }
        public string? CondicionesServicio { get; set; }
        public string? Observaciones { get; set; }
    }
}
