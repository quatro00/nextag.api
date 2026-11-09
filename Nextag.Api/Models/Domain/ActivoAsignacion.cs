using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class ActivoAsignacion
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid ActivoId { get; set; }

    public Guid ResponsableId { get; set; }

    public DateTime FechaAsignacion { get; set; }

    public string? MotivoAsignacion { get; set; }

    public string? ObservacionesAsignacion { get; set; }

    public DateTime? FechaDevolucion { get; set; }

    public string? MotivoDevolucion { get; set; }

    public string? CondicionDevolucion { get; set; }

    public string? ObservacionesDevolucion { get; set; }

    public Guid? UsuarioDevolucion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioModificacion { get; set; }

    public virtual Activo Activo { get; set; } = null!;

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual Responsable Responsable { get; set; } = null!;
}
