namespace Nextag.Api.Models.Dto.Ubicaciones
{
    public class UbicacionDetalleDto : UbicacionListDto
    {
        public DateTime FechaCreacion { get; set; }

        public Guid UsuarioCreacion { get; set; }

        public DateTime? FechaActualizacion { get; set; }

        public Guid? UsuarioModificacion { get; set; }
    }
}
