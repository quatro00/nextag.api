namespace Nextag.Api.Models.Dto.Contratos
{
    public class ContratoListDto
    {
        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public string NumeroContrato { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public Guid TipoContratoId { get; set; }
        public string TipoContratoNombre { get; set; } = string.Empty;
        public int EstadoContratoId { get; set; }
        public string EstadoContratoClave { get; set; } = string.Empty;
        public string EstadoContratoNombre { get; set; } = string.Empty;
        public Guid? ClienteId { get; set; }
        public string? ClienteNombre { get; set; }
        public Guid? ProveedorId { get; set; }
        public string? ProveedorNombre { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public bool RenovacionAutomatica { get; set; }
        public decimal? Monto { get; set; }
        public string Moneda { get; set; } = string.Empty;
        public string? PeriodicidadPago { get; set; }
        public bool Activo { get; set; }
        public int TotalActivosAsociados { get; set; }
        public int? DiasRestantes { get; set; }
        public bool EstaVencido { get; set; }
        public bool ProximoAVencer { get; set; }
        public bool PendienteInicio { get; set; }
    }
}
