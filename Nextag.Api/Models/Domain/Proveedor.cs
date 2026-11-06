using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class Proveedor
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string Clave { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? RazonSocial { get; set; }

    public string? Rfc { get; set; }

    public string? Correo { get; set; }

    public string? Telefono { get; set; }

    public string? SitioWeb { get; set; }

    public string? Direccion { get; set; }

    public string? Colonia { get; set; }

    public string? Municipio { get; set; }

    public string? Estado { get; set; }

    public string? Pais { get; set; }

    public string? CodigoPostal { get; set; }

    public string? Observaciones { get; set; }

    public bool? Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioModificacion { get; set; }

    public virtual ICollection<Activo> ActivoProveedorNavigations { get; set; } = new List<Activo>();

    public virtual ICollection<Activo> ActivoProveedors { get; set; } = new List<Activo>();

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual ICollection<ProveedorContacto> ProveedorContactos { get; set; } = new List<ProveedorContacto>();

    public virtual ICollection<ProveedorTipo> ProveedorTipos { get; set; } = new List<ProveedorTipo>();
}
