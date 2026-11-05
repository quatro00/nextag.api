namespace Nextag.Api.Models.Dto.Marcas
{
    public class MarcaDetalleDto : MarcaListDto
    {
        public DateTime FechaCreacion { get; set; }

        public Guid UsuarioCreacion { get; set; }

        public DateTime? FechaActualizacion { get; set; }

        public Guid? UsuarioModificacion { get; set; }
    }
}
