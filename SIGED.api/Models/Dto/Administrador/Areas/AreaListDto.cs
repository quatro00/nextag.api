namespace SIGED.api.Models.Dto.Administrador.Areas
{
    public class AreaListDto
    {
        public Guid Id { get; set; }
        public string Clave { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
