using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class Ticket
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public long Numero { get; set; }

    public string Folio { get; set; } = null!;

    public Guid? ActivoId { get; set; }

    public Guid? CategoriaTicketId { get; set; }

    public int PrioridadTicketId { get; set; }

    public int EstadoTicketId { get; set; }

    public string Origen { get; set; } = null!;

    public string Titulo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public Guid? SolicitanteUsuarioId { get; set; }

    public string? SolicitanteNombre { get; set; }

    public string? SolicitanteCorreo { get; set; }

    public string? SolicitanteTelefono { get; set; }

    public DateTime FechaApertura { get; set; }

    public DateTime? FechaPrimeraAtencion { get; set; }

    public DateTime? FechaResolucion { get; set; }

    public DateTime? FechaCierre { get; set; }

    public DateTime? FechaCancelacion { get; set; }

    public string? MotivoCancelacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid? UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioModificacion { get; set; }

    public virtual Activo? Activo { get; set; }

    public virtual ICollection<ArchivoAdjunto> ArchivoAdjuntos { get; set; } = new List<ArchivoAdjunto>();

    public virtual CategoriaTicket? CategoriaTicket { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual EstadoTicket EstadoTicket { get; set; } = null!;

    public virtual ICollection<OrdenTrabajo> OrdenTrabajos { get; set; } = new List<OrdenTrabajo>();

    public virtual PrioridadTicket PrioridadTicket { get; set; } = null!;

    public virtual Usuario? SolicitanteUsuario { get; set; }

    public virtual ICollection<TicketAsignacion> TicketAsignacions { get; set; } = new List<TicketAsignacion>();

    public virtual ICollection<TicketEstadoHistorial> TicketEstadoHistorials { get; set; } = new List<TicketEstadoHistorial>();

    public virtual ICollection<TicketSeguimiento> TicketSeguimientos { get; set; } = new List<TicketSeguimiento>();
}
