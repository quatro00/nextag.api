using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Etiquetas;

public class EtiquetaDesvincularDto
{
    [Required, MaxLength(300)] public string Motivo { get; set; } = string.Empty;
}
