namespace Nextag.Api.Models.Dto.EstadosContrato
{
    public class EstadoContratoDto
    {
        public int Id { get; set; }
        public string Clave { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public int Orden { get; set; }
    }
}
