using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.EquipoSoporteIntegrantes
{
    public class EquipoSoporteIntegranteCrearDto
    {
        [Required]
        public Guid? UsuarioId { get; set; }
    }
}
