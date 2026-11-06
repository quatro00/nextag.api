using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class ProveedorTipo
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid ProveedorId { get; set; }

    public int TipoProveedorId { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid UsuarioCreacion { get; set; }

    public virtual Proveedor Proveedor { get; set; } = null!;

    public virtual TipoProveedor TipoProveedor { get; set; } = null!;
}
