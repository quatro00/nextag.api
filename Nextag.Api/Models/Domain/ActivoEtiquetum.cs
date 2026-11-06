using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class ActivoEtiquetum
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid ActivoId { get; set; }

    public Guid EtiquetaId { get; set; }

    public DateTime FechaVinculacion { get; set; }

    public Guid UsuarioVinculacion { get; set; }

    public DateTime? FechaDesvinculacion { get; set; }

    public Guid? UsuarioDesvinculacion { get; set; }

    public string? MotivoDesvinculacion { get; set; }

    public virtual Activo Activo { get; set; } = null!;

    public virtual Etiquetum E { get; set; } = null!;
}
