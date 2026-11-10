using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class TicketAsignacion
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid TicketId { get; set; }

    public Guid EquipoSoporteId { get; set; }

    public Guid? UsuarioTecnicoId { get; set; }

    public DateTime FechaAsignacion { get; set; }

    public DateTime? FechaFin { get; set; }

    public string? Motivo { get; set; }

    public Guid UsuarioCreacion { get; set; }

    public virtual EquipoSoporte E { get; set; } = null!;

    public virtual Ticket Ticket { get; set; } = null!;

    public virtual Usuario? UsuarioTecnico { get; set; }
}
