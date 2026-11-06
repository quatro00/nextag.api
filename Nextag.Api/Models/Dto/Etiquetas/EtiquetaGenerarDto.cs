using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Etiquetas;

public class EtiquetaGenerarDto
{
    [Range(1, 250)] public int Cantidad { get; set; } = 1;
    public bool TieneNfc { get; set; } = true;
    public bool TieneQr { get; set; } = true;
    public bool EsAntimetal { get; set; }
}
