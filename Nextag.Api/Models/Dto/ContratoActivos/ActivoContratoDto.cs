namespace Nextag.Api.Models.Dto.ContratoActivos
{
    public class ActivoContratoDto
    {
        public Guid Id { get; set; }
        public Guid ActivoId { get; set; }
        public Guid ContratoId { get; set; }
        public string NumeroContrato { get; set; } = string.Empty;
        public string ContratoNombre { get; set; } = string.Empty;
        public Guid TipoContratoId { get; set; }
        public string TipoContratoNombre { get; set; } = string.Empty;
        public int EstadoContratoId { get; set; }
        public string EstadoContratoClave { get; set; } = string.Empty;
        public string EstadoContratoNombre { get; set; } = string.Empty;
        public Guid? ClienteId { get; set; }
        public string? ClienteNombre { get; set; }
        public Guid? ProveedorId { get; set; }
        public string? ProveedorNombre { get; set; }
        public DateTime FechaInicioContrato { get; set; }
        public DateTime? FechaFinContrato { get; set; }
        public bool ContratoActivo { get; set; }
        public bool ContratoVencido { get; set; }
        public bool CoberturaVigente { get; set; }
        public DateTime FechaInclusion { get; set; }
        public DateTime? FechaExclusion { get; set; }
        public string? MotivoExclusion { get; set; }
        public string? Observaciones { get; set; }
        public bool AsociacionVigente { get; set; }
    }
}
