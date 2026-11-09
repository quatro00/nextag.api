namespace Nextag.Api.Models.Dto.ActivoMovimientos
{
    public class ActivoMovimientoDto
    {
        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }

        public Guid ActivoId { get; set; }
        public string ActivoCodigo { get; set; } = "";
        public string ActivoNombre { get; set; } = "";
        public string? ActivoNumeroSerie { get; set; }

        public int TipoMovimientoActivoId { get; set; }
        public string TipoMovimientoClave { get; set; } = "";
        public string TipoMovimientoNombre { get; set; } = "";

        public DateTime FechaMovimiento { get; set; }
        public string? Motivo { get; set; }
        public string? Observaciones { get; set; }

        // Origen interno
        public Guid? SucursalOrigenId { get; set; }
        public Guid? UbicacionOrigenId { get; set; }
        public string? SucursalOrigenNombre { get; set; }
        public string? UbicacionOrigenNombre { get; set; }

        // Origen externo
        public bool EsOrigenExterno { get; set; }
        public string? OrigenExternoNombre { get; set; }
        public string? OrigenExternoDireccion { get; set; }

        // Destino interno
        public Guid? SucursalDestinoId { get; set; }
        public Guid? UbicacionDestinoId { get; set; }
        public string? SucursalDestinoNombre { get; set; }
        public string? UbicacionDestinoNombre { get; set; }

        // Destino externo
        public bool EsDestinoExterno { get; set; }
        public string? DestinoExternoNombre { get; set; }
        public string? DestinoExternoDireccion { get; set; }
        public Guid? ProveedorDestinoId { get; set; }

        // Salida relacionada
        public Guid? MovimientoOrigenId { get; set; }

        // Estados
        public int EstadoActivoOrigenId { get; set; }
        public int EstadoActivoDestinoId { get; set; }

        // Referencia futura
        public string? TipoReferencia { get; set; }
        public Guid? ReferenciaId { get; set; }

        // Auditoría y anulación
        public bool Anulado { get; set; }
        public DateTime? FechaAnulacion { get; set; }
        public Guid? UsuarioAnulacion { get; set; }
        public string? MotivoAnulacion { get; set; }

        public DateTime FechaCreacion { get; set; }
        public Guid UsuarioCreacion { get; set; }
    }
}
