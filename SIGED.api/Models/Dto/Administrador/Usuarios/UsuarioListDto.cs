namespace SIGED.api.Models.Dto.Administrador.Usuarios
{
    public class UsuarioListDto
    {
        public Guid Id { get; set; }
        public string AspNetUserId { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime? FechaUltimoAcceso { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
