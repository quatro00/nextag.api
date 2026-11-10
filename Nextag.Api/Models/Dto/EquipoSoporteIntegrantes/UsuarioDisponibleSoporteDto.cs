namespace Nextag.Api.Models.Dto.EquipoSoporteIntegrantes
{
    public class UsuarioDisponibleSoporteDto
    {
        public Guid Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Apellidos { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;
    }
}
