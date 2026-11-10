using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class OrdenTrabajo
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid? TicketId { get; set; }

    public Guid? ActivoId { get; set; }

    public string Origen { get; set; } = null!;

    public Guid? ReferenciaOrigenId { get; set; }

    public int EstadoOrdenTrabajoId { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public Guid? EquipoSoporteId { get; set; }

    public Guid? UsuarioTecnicoId { get; set; }

    public DateTime? FechaProgramada { get; set; }

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public string? Resultado { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioModificacion { get; set; }

    public virtual Activo? Activo { get; set; }

    public virtual ICollection<ArchivoAdjunto> ArchivoAdjuntos { get; set; } = new List<ArchivoAdjunto>();

    public virtual EquipoSoporte? E { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual EstadoOrdenTrabajo EstadoOrdenTrabajo { get; set; } = null!;

    public virtual ICollection<OrdenTrabajoActividad> OrdenTrabajoActividads { get; set; } = new List<OrdenTrabajoActividad>();

    public virtual Ticket? Ticket { get; set; }

    public virtual Usuario? UsuarioTecnico { get; set; }
}
