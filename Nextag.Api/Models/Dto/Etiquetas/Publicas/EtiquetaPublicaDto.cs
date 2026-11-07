namespace Nextag.Api.Models.Dto.Etiquetas.Publicas
{
    public class EtiquetaPublicaDto
    {
        public string CodigoEtiqueta { get; set; } = string.Empty;

        public string Estatus { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;

        public EmpresaPublicaDto? Empresa { get; set; }

        public ActivoPublicoDto? Activo { get; set; }

        public bool PermitirTicketPublico { get; set; }
    }

    public class EmpresaPublicaDto
    {
        public string Nombre { get; set; } = string.Empty;

        public string? LogoUrl { get; set; }
    }

    public class ActivoPublicoDto
    {
        public string? Nombre { get; set; }

        public string? Codigo { get; set; }

        public string? NumeroInventario { get; set; }

        public string? NumeroSerie { get; set; }

        public string? Categoria { get; set; }

        public string? Marca { get; set; }

        public string? Modelo { get; set; }

        public string? Sucursal { get; set; }

        public string? Ubicacion { get; set; }

        public string? Estado { get; set; }
    }
}
