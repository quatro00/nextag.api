namespace Nextag.Api.Models.Dto.EquipoSoporteIntegrantes
{
    public class EquipoSoporteIntegranteDto
    {
        public Guid Id { get; set; }

        public Guid EmpresaId { get; set; }

        public Guid EquipoSoporteId { get; set; }

        public Guid UsuarioId { get; set; }

        public string UsuarioNombre { get; set; } = string.Empty;

        public string UsuarioCorreo { get; set; } = string.Empty;

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public Guid UsuarioCreacion { get; set; }

        public DateTime? FechaActualizacion { get; set; }

        public Guid? UsuarioModificacion { get; set; }
    }
}
