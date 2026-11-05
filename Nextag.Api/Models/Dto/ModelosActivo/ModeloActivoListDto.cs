namespace Nextag.Api.Models.Dto.ModelosActivo
{
    public class ModeloActivoListDto
    {
        public Guid Id { get; set; }

        public Guid EmpresaId { get; set; }

        public Guid MarcaId { get; set; }

        public string MarcaNombre { get; set; } = string.Empty;

        public Guid CategoriaActivoId { get; set; }

        public string CategoriaActivoNombre { get; set; } = string.Empty;

        public string Clave { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Activo { get; set; }
    }
}
