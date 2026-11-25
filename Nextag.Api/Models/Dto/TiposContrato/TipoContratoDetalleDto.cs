namespace Nextag.Api.Models.Dto.TiposContrato
{
    public class TipoContratoDetalleDto : TipoContratoListDto
    {
        public DateTime FechaCreacion { get; set; }
        public Guid UsuarioCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public Guid? UsuarioModificacion { get; set; }
    }
}
