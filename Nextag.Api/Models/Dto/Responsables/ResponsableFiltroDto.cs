namespace Nextag.Api.Models.Dto.Responsables
{
    public class ResponsableFiltroDto
    {
        public string? Buscar { get; set; }

        public string? Tipo { get; set; }

        public bool? Activo { get; set; }

        public Guid? SucursalId { get; set; }

        public Guid? AreaResponsableId { get; set; }

        public int Pagina { get; set; } = 1;

        public int TamanoPagina { get; set; } = 20;
    }
}
