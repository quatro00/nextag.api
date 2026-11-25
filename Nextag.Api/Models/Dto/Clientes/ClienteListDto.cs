namespace Nextag.Api.Models.Dto.Clientes
{
    public class ClienteListDto
    {
        public Guid Id { get; set; }

        public Guid EmpresaId { get; set; }

        public string Clave { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? RazonSocial { get; set; }

        public string? Rfc { get; set; }

        public string? Correo { get; set; }

        public string? Telefono { get; set; }

        public string? ContactoPrincipal { get; set; }

        public string? TelefonoContacto { get; set; }

        public string? CorreoContacto { get; set; }

        public bool Activo { get; set; }
    }
}
