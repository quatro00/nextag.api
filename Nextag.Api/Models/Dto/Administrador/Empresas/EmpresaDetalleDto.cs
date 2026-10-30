namespace Nextag.Api.Models.Dto.Administrador.Empresas
{
    public class EmpresaDetalleDto
    {
        public Guid Id { get; set; }

        public string Clave { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? RazonSocial { get; set; }
        public string? Rfc { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string? SitioWeb { get; set; }
        public string? LogoUrl { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public Guid UsuarioCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public Guid? UsuarioModificacion { get; set; }
    }
}
