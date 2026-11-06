using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class ProveedorContacto
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid ProveedorId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Cargo { get; set; }

    public string? Correo { get; set; }

    public string? Telefono { get; set; }

    public string? Celular { get; set; }

    public bool EsPrincipal { get; set; }

    public bool? Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioModificacion { get; set; }

    public virtual Proveedor Proveedor { get; set; } = null!;
}
