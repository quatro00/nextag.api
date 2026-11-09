namespace Nextag.Api.Models.Dto.ActivoMovimientos
{
    public class TipoMovimientoActivoDto
    {
        public int Id { get; set; }

        public string Clave { get; set; } = "";

        public string Nombre { get; set; } = "";

        public string? Descripcion { get; set; }
    }
}
