using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Etiquetas;

public class EtiquetaFiltroDto
{
    [MaxLength(100)] public string? Buscar { get; set; }
    public string? Estatus { get; set; } // DISPONIBLE, VINCULADA, RETIRADA
    public bool? TieneNfc { get; set; }
    public bool? TieneQr { get; set; }
    [Range(1, 1000000)] public int Pagina { get; set; } = 1;
    [Range(1, 100)] public int TamanoPagina { get; set; } = 20;
}
