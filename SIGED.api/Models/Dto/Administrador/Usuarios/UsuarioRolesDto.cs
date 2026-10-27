using System.ComponentModel.DataAnnotations;

namespace SIGED.api.Models.Dto.Administrador.Usuarios
{
    public class UsuarioRolesDto
    {
        [MinLength(1)]
        public List<string> Roles { get; set; } = new();
    }
}
