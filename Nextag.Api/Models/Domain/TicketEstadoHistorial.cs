using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class TicketEstadoHistorial
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid TicketId { get; set; }

    public int? EstadoAnteriorId { get; set; }

    public int EstadoNuevoId { get; set; }

    public string? Motivo { get; set; }

    public DateTime FechaCambio { get; set; }

    public Guid? UsuarioId { get; set; }

    public virtual EstadoTicket? EstadoAnterior { get; set; }

    public virtual EstadoTicket EstadoNuevo { get; set; } = null!;

    public virtual Ticket Ticket { get; set; } = null!;
}
