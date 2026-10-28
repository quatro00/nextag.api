using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class Empresa
{
    public Guid Id { get; set; }

    public string Clave { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? RazonSocial { get; set; }

    public string? Rfc { get; set; }

    public string? Correo { get; set; }

    public string? Telefono { get; set; }

    public string? SitioWeb { get; set; }

    public string? LogoUrl { get; set; }

    public bool? Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioModificacion { get; set; }

    public virtual ICollection<UsuarioEmpresa> UsuarioEmpresas { get; set; } = new List<UsuarioEmpresa>();
}
