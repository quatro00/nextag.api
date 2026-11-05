namespace Nextag.Api.Models.Dto.EstadosActivo
{
    public class EstadoActivoDto
    {
        public int Id { get; set; }

        public string Clave { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Activo { get; set; }
    }
}
