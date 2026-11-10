namespace Nextag.Api.Models.Dto.CategoriasTicket
{
    public class CategoriaTicketDetalleDto : CategoriaTicketListDto
    {
        public DateTime FechaCreacion { get; set; }

        public Guid UsuarioCreacion { get; set; }

        public DateTime? FechaActualizacion { get; set; }

        public Guid? UsuarioModificacion { get; set; }
    }
}
