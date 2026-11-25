using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.ContratoRenovaciones
{
    public class ContratoRenovacionGuardarDto
    {
        [Required]
        public DateTime? FechaInicioNueva { get; set; }

        [Required]
        public DateTime? FechaFinNueva { get; set; }

        [Range(
            typeof(decimal),
            "0",
            "9999999999999999.99")]
        public decimal? MontoNuevo { get; set; }

        public bool ConservarMontoActual { get; set; } = true;

        [Required]
        [MaxLength(500)]
        public string Motivo { get; set; } = string.Empty;

        public string? Observaciones { get; set; }

        // Valores esperados para detectar si el contrato
        // fue modificado desde que se abrió el formulario.

        [Required]
        public DateTime? FechaInicioEsperada { get; set; }

        [Required]
        public DateTime? FechaFinEsperada { get; set; }

        public decimal? MontoEsperado { get; set; }

        [Required]
        public int? UltimaRenovacionEsperada { get; set; }

        public DateTime? FechaActualizacionEsperada { get; set; }
    }
}
