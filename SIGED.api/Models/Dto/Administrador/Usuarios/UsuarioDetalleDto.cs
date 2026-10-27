namespace SIGED.api.Models.Dto.Administrador.Usuarios
{
    public class UsuarioDetalleDto
    {
        public Guid Id { get; set; }
        public string AspNetUserId { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Celular { get; set; }
        public string? Pais { get; set; }
        public string? CodigoPais { get; set; }
        public string? Empresa { get; set; }
        public string? DominioPrincipal { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaUltimoAcceso { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
