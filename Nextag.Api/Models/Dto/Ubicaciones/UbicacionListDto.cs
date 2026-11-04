namespace Nextag.Api.Models.Dto.Ubicaciones
{
    public class UbicacionListDto
    {
        public Guid Id { get; set; }

        public Guid EmpresaId { get; set; }

        public Guid SucursalId { get; set; }

        public string SucursalNombre { get; set; } = string.Empty;

        public Guid? TipoUbicacionId { get; set; }

        public string? TipoUbicacionNombre { get; set; }

        public Guid? UbicacionPadreId { get; set; }

        public string? UbicacionPadreNombre { get; set; }

        public string Clave { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Activo { get; set; }
    }
}
