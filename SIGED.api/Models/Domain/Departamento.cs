using System;
using System.Collections.Generic;

namespace SIGED.api.Models.Domain;

public partial class Departamento
{
    public Guid Id { get; set; }

    public Guid AreaId { get; set; }

    public string Clave { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool? Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioCreacionId { get; set; }

    public Guid? UsuarioActualizacionId { get; set; }

    public virtual Area Area { get; set; } = null!;
}
