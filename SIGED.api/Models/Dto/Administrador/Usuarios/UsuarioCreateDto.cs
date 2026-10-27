using System.ComponentModel.DataAnnotations;

namespace SIGED.api.Models.Dto.Administrador.Usuarios
{
    public class UsuarioCreateDto
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string Apellidos { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public string? Telefono { get; set; }
        public string? Celular { get; set; }
        public string? Pais { get; set; }
        public string? CodigoPais { get; set; }
        public string? Empresa { get; set; }
        public string? DominioPrincipal { get; set; }

        [MinLength(1)]
        public List<string> Roles { get; set; } = new();
    }
}
