namespace Nextag.Api.Models.Dto.EquiposSoporte
{
    public class EquipoSoporteListDto
    {
        public Guid Id { get; set; }

        public Guid EmpresaId { get; set; }

        public string Clave { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public string Tipo { get; set; } = string.Empty;

        public Guid? ProveedorId { get; set; }

        public string? ProveedorNombre { get; set; }

        public string? CorreoContacto { get; set; }

        public bool Activo { get; set; }

        public int TotalIntegrantes { get; set; }
    }
}
