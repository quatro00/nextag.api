using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class CategoriaActivo
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid? CategoriaPadreId { get; set; }

    public string Clave { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool? Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioModificacion { get; set; }

    public virtual CategoriaActivo? CategoriaActivoNavigation { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual ICollection<CategoriaActivo> InverseCategoriaActivoNavigation { get; set; } = new List<CategoriaActivo>();

    public virtual ICollection<ModeloActivo> ModeloActivos { get; set; } = new List<ModeloActivo>();
}
