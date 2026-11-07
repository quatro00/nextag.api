using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class EtiquetaConfiguracion
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public bool? MostrarNombreActivo { get; set; }

    public bool? MostrarCodigoActivo { get; set; }

    public bool? MostrarNumeroInventario { get; set; }

    public bool MostrarNumeroSerie { get; set; }

    public bool? MostrarCategoria { get; set; }

    public bool? MostrarMarcaModelo { get; set; }

    public bool MostrarSucursal { get; set; }

    public bool MostrarUbicacion { get; set; }

    public bool MostrarResponsable { get; set; }

    public bool? MostrarEstadoActivo { get; set; }

    public bool? MostrarDatosEmpresa { get; set; }

    public bool PermitirTicketPublico { get; set; }

    public bool? Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid? UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioModificacion { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;
}
