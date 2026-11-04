namespace Nextag.Api.Models.Dto.Sucursales
{
    public class SucursalDetalleDto : SucursalListDto
    {
        public string? Descripcion { get; set; }

        public string? Direccion { get; set; }

        public string? Colonia { get; set; }

        public string? CodigoPostal { get; set; }

        public string? Pais { get; set; }

        public DateTime FechaCreacion { get; set; }

        public Guid UsuarioCreacion { get; set; }

        public DateTime? FechaActualizacion { get; set; }

        public Guid? UsuarioModificacion { get; set; }
    }
}
