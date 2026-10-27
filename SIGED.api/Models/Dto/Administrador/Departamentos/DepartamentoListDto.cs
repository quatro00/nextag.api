namespace SIGED.api.Models.Dto.Administrador.Departamentos
{
    public class DepartamentoListDto
    {
        public Guid Id { get; set; }
        public Guid AreaId { get; set; }
        public string AreaClave { get; set; } = string.Empty;
        public string AreaNombre { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
