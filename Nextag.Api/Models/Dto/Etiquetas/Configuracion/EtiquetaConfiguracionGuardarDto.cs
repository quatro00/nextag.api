using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Etiquetas.Configuracion
{
    public class EtiquetaConfiguracionGuardarDto
    {
        [Required]
        public bool? Activo { get; set; }

        [Required]
        public bool? PermitirTicketPublico { get; set; }

        [Required]
        public bool? MostrarNombreActivo { get; set; }

        [Required]
        public bool? MostrarCodigoActivo { get; set; }

        [Required]
        public bool? MostrarNumeroInventario { get; set; }

        [Required]
        public bool? MostrarNumeroSerie { get; set; }

        [Required]
        public bool? MostrarCategoria { get; set; }

        [Required]
        public bool? MostrarMarcaModelo { get; set; }

        [Required]
        public bool? MostrarSucursal { get; set; }

        [Required]
        public bool? MostrarUbicacion { get; set; }

        [Required]
        public bool? MostrarResponsable { get; set; }

        [Required]
        public bool? MostrarEstadoActivo { get; set; }

        [Required]
        public bool? MostrarDatosEmpresa { get; set; }
    }
}
