namespace Nextag.Api.Models.Dto.TiposUbicacion
{
    public class TipoUbicacionListDto
    {
        public Guid Id { get; set; }

        public Guid EmpresaId { get; set; }

        public string Clave { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Activo { get; set; }
    }
}
