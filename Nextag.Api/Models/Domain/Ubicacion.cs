using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class Ubicacion
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid SucursalId { get; set; }

    public Guid? TipoUbicacionId { get; set; }

    public Guid? UbicacionPadreId { get; set; }

    public string Clave { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool? Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioModificacion { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual ICollection<Ubicacion> InverseUbicacionNavigation { get; set; } = new List<Ubicacion>();

    public virtual Sucursal Sucursal { get; set; } = null!;

    public virtual TipoUbicacion? TipoUbicacion { get; set; }

    public virtual Ubicacion? UbicacionNavigation { get; set; }
}
