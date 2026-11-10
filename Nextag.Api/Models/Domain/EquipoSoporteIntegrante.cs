using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class EquipoSoporteIntegrante
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid EquipoSoporteId { get; set; }

    public Guid UsuarioId { get; set; }

    public bool EsLider { get; set; }

    public bool? Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioModificacion { get; set; }

    public virtual EquipoSoporte E { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
