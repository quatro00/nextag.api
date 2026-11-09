using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class Responsable
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string Tipo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Codigo { get; set; }

    public string? Puesto { get; set; }

    public string? Departamento { get; set; }

    public Guid? SucursalId { get; set; }

    public Guid? AreaResponsableId { get; set; }

    public string? Contacto { get; set; }

    public string? Correo { get; set; }

    public string? Telefono { get; set; }

    public string? Observaciones { get; set; }

    public bool? Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioModificacion { get; set; }

    public virtual ICollection<ActivoAsignacion> ActivoAsignacions { get; set; } = new List<ActivoAsignacion>();

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual ICollection<Responsable> InverseResponsableNavigation { get; set; } = new List<Responsable>();

    public virtual Responsable? ResponsableNavigation { get; set; }

    public virtual Sucursal? Sucursal { get; set; }
}
