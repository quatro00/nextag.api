namespace Nextag.Api.Models.Dto.Activos;

public class ActivoPaginaDto
{
    public List<ActivoListDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
}
