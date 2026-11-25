using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.ContratoActivos
{
    public class ContratoActivoFiltroDto
    {
        public string? Buscar { get; set; }
        public Guid? CategoriaActivoId { get; set; }
        public Guid? SucursalId { get; set; }
        public Guid? UbicacionId { get; set; }
        public int? EstadoActivoId { get; set; }
        [Range(1, int.MaxValue)]
        public int Pagina { get; set; } = 1;

        [Range(1, 200)]
        public int RegistrosPorPagina { get; set; } = 25;
    }
}
