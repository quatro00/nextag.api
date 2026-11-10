namespace Nextag.Api.Models.Dto.EquiposSoporte
{
    public class EquipoSoporteDetalleDto : EquipoSoporteListDto
    {
        public DateTime FechaCreacion { get; set; }

        public Guid UsuarioCreacion { get; set; }

        public DateTime? FechaActualizacion { get; set; }

        public Guid? UsuarioModificacion { get; set; }
    }
}
