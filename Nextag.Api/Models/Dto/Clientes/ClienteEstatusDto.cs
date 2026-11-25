using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Clientes
{
    public class ClienteEstatusDto
    {
        [Required]
        public bool? Activo { get; set; }
    }
}
