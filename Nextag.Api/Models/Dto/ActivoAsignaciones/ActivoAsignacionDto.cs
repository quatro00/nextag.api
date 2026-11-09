namespace Nextag.Api.Models.Dto.ActivoAsignaciones
{
    public class ActivoAsignacionDto
    {
        public Guid Id { get; set; }

        public Guid EmpresaId { get; set; }

        // =====================================
        // ACTIVO
        // =====================================

        public Guid ActivoId { get; set; }

        public string ActivoCodigo { get; set; } = string.Empty;

        public string ActivoNombre { get; set; } = string.Empty;

        public string? ActivoNumeroSerie { get; set; }

        public string? ActivoNumeroInventario { get; set; }

        public Guid SucursalId { get; set; }

        public string SucursalNombre { get; set; } = string.Empty;

        public Guid? UbicacionId { get; set; }

        public string? UbicacionNombre { get; set; }

        // =====================================
        // RESPONSABLE
        // =====================================

        public Guid ResponsableId { get; set; }

        public string ResponsableNombre { get; set; } = string.Empty;

        public string ResponsableTipo { get; set; } = string.Empty;

        public string? ResponsableCodigo { get; set; }

        public Guid? AreaResponsableId { get; set; }

        public string? AreaResponsableNombre { get; set; }

        // =====================================
        // ASIGNACION
        // =====================================

        public DateTime FechaAsignacion { get; set; }

        public string? MotivoAsignacion { get; set; }

        public string? ObservacionesAsignacion { get; set; }

        // =====================================
        // DEVOLUCION
        // =====================================

        public DateTime? FechaDevolucion { get; set; }

        public string? MotivoDevolucion { get; set; }

        public string? CondicionDevolucion { get; set; }

        public string? ObservacionesDevolucion { get; set; }

        public Guid? UsuarioDevolucion { get; set; }

        // =====================================
        // ESTATUS
        // =====================================

        public bool Vigente { get; set; }

        // =====================================
        // AUDITORIA
        // =====================================

        public DateTime FechaCreacion { get; set; }

        public Guid UsuarioCreacion { get; set; }

        public DateTime? FechaActualizacion { get; set; }

        public Guid? UsuarioModificacion { get; set; }
    }
}
