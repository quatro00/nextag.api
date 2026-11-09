namespace Nextag.Api.Models.Dto.Responsables
{
    public class ResponsableDto
    {
        public Guid Id { get; set; }

        public Guid EmpresaId { get; set; }

        public string Tipo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? Codigo { get; set; }

        public string? Puesto { get; set; }

        public string? Departamento { get; set; }

        public Guid? SucursalId { get; set; }

        public string? SucursalNombre { get; set; }

        public Guid? AreaResponsableId { get; set; }

        public string? AreaResponsableNombre { get; set; }

        public string? Contacto { get; set; }

        public string? Correo { get; set; }

        public string? Telefono { get; set; }

        public string? Observaciones { get; set; }

        public bool Activo { get; set; }

        public int TotalPersonas { get; set; }

        public DateTime FechaCreacion { get; set; }

        public Guid UsuarioCreacion { get; set; }

        public DateTime? FechaActualizacion { get; set; }

        public Guid? UsuarioModificacion { get; set; }
    }
}
