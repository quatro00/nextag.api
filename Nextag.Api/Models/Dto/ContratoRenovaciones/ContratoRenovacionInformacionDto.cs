namespace Nextag.Api.Models.Dto.ContratoRenovaciones
{
    public class ContratoRenovacionInformacionDto
    {
        public Guid ContratoId { get; set; }
        public string NumeroContrato { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int EstadoContratoId { get; set; }
        public int EstadoEfectivoId { get; set; }
        public string EstadoContratoNombre { get; set; } = string.Empty;
        public DateTime FechaInicioActual { get; set; }
        public DateTime? FechaFinActual { get; set; }
        public decimal? MontoActual { get; set; }
        public string Moneda { get; set; } = string.Empty;
        public bool RenovacionAutomatica { get; set; }
        public int? DiasAvisoNoRenovacion { get; set; }
        public int TotalRenovaciones { get; set; }
        public int UltimaRenovacion { get; set; }
        public int SiguienteRenovacion { get; set; }
        public int TotalActivosAsociados { get; set; }
        public bool Activo { get; set; }
        public bool EstaVencido { get; set; }
        public bool PuedeRenovar { get; set; }
        public string? MotivoNoDisponible { get; set; }
        public DateTime? FechaInicioSugerida { get; set; }
        public DateTime? FechaFinSugerida { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }
}
