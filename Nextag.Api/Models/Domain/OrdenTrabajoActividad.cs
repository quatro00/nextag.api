using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class OrdenTrabajoActividad
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid OrdenTrabajoId { get; set; }

    public string Descripcion { get; set; } = null!;

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public int? MinutosTrabajados { get; set; }

    public Guid? UsuarioTecnicoId { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid UsuarioCreacion { get; set; }

    public virtual OrdenTrabajo OrdenTrabajo { get; set; } = null!;

    public virtual Usuario? UsuarioTecnico { get; set; }
}
