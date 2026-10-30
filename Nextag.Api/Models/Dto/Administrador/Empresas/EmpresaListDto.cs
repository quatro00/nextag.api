namespace Nextag.Api.Models.Dto.Administrador.Empresas
{
    public class EmpresaListDto
    {
        public Guid Id { get; set; }
        public string Clave { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? RazonSocial { get; set; }
        public string? Rfc { get; set; }
        public string? Correo { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
