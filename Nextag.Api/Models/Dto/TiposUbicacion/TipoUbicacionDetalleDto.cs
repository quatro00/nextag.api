namespace Nextag.Api.Models.Dto.TiposUbicacion
{
    public class TipoUbicacionDetalleDto : TipoUbicacionListDto
    {
        public DateTime FechaCreacion { get; set; }

        public Guid UsuarioCreacion { get; set; }

        public DateTime? FechaActualizacion { get; set; }

        public Guid? UsuarioModificacion { get; set; }
    }
}
