namespace Nextag.Api.Models.Dto.CategoriasActivo
{
    public class CategoriaActivoListDto
    {
        public Guid Id { get; set; }

        public Guid EmpresaId { get; set; }

        public Guid? CategoriaPadreId { get; set; }

        public string? CategoriaPadreNombre { get; set; }

        public string Clave { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Activo { get; set; }
    }
}
