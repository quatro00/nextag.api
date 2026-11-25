namespace Nextag.Api.Models.Dto.Contratos
{
    public class ContratoDetalleDto : ContratoListDto
    {
        public string? Descripcion { get; set; }
        public DateTime? FechaFirma { get; set; }
        public int? DiasAvisoNoRenovacion { get; set; }
        public int? DiaPago { get; set; }
        public bool IncluyeImpuestos { get; set; }
        public bool? IncluyePreventivo { get; set; }
        public bool? IncluyeCorrectivo { get; set; }
        public bool? IncluyeRefacciones { get; set; }
        public bool? IncluyeManoObra { get; set; }
        public decimal? TiempoRespuestaHoras { get; set; }
        public string? HorarioAtencion { get; set; }
        public int? ServiciosIncluidos { get; set; }
        public string? CondicionesServicio { get; set; }
        public DateTime? FechaCancelacion { get; set; }
        public string? MotivoCancelacion { get; set; }
        public Guid? UsuarioCancelacion { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public string? Observaciones { get; set; }
        public int TotalRenovaciones { get; set; }
        public DateTime FechaCreacion { get; set; }
        public Guid UsuarioCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public Guid? UsuarioModificacion { get; set; }
    }
}
