using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class ModeloActivo
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid MarcaId { get; set; }

    public Guid CategoriaActivoId { get; set; }

    public string Clave { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool? Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioModificacion { get; set; }

    public virtual ICollection<Activo> Activos { get; set; } = new List<Activo>();

    public virtual CategoriaActivo CategoriaActivo { get; set; } = null!;

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual Marca Marca { get; set; } = null!;
}
