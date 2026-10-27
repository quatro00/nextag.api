using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Administrador.Usuarios
{
    public class UsuarioRolesDto
    {
        [MinLength(1)]
        public List<string> Roles { get; set; } = new();
    }
}
