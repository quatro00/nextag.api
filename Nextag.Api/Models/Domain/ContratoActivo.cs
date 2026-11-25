using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class ContratoActivo
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid ContratoId { get; set; }

    public Guid ActivoId { get; set; }

    public DateTime FechaInclusion { get; set; }

    public DateTime? FechaExclusion { get; set; }

    public string? MotivoExclusion { get; set; }

    public string? Observaciones { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioModificacion { get; set; }

    public virtual Activo Activo { get; set; } = null!;

    public virtual Contrato Contrato { get; set; } = null!;

    public virtual Empresa Empresa { get; set; } = null!;
}
