namespace Nextag.Api.Models.Dto.ActivoMovimientos
{
    public class ActivoSituacionDto
    {
        public Guid ActivoId { get; set; }

        public string ActivoCodigo { get; set; } = "";

        public string ActivoNombre { get; set; } = "";

        public int EstadoActivoId { get; set; }

        public string EstadoActivoNombre { get; set; } = "";

        public bool EstaFuera { get; set; }

        // Ubicación administrativa
        public Guid SucursalId { get; set; }
        public string SucursalNombre { get; set; } = "";

        public Guid? UbicacionId { get; set; }
        public string? UbicacionNombre { get; set; }

        // Ubicación física externa
        public string? UbicacionExternaNombre { get; set; }

        public string? UbicacionExternaDireccion { get; set; }

        public Guid? ProveedorExternoId { get; set; }

        // Salida abierta
        public Guid? MovimientoSalidaId { get; set; }

        public DateTime? FechaSalida { get; set; }
    }
}
