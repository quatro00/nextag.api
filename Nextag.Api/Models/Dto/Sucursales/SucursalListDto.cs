namespace Nextag.Api.Models.Dto.Sucursales
{
    public class SucursalListDto
    {
        public Guid Id { get; set; }

        public Guid EmpresaId { get; set; }

        public string Clave { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? Municipio { get; set; }

        public string? Estado { get; set; }

        public string? Telefono { get; set; }

        public string? Correo { get; set; }

        public bool Activo { get; set; }
    }
}
