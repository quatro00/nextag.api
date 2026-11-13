using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class TicketAdjuntoToken
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid TicketId { get; set; }

    public string TokenHash { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaExpiracion { get; set; }

    public int MaximoArchivos { get; set; }

    public int ArchivosCargados { get; set; }

    public bool Revocado { get; set; }

    public DateTime? FechaRevocacion { get; set; }

    public DateTime? UltimaUtilizacion { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual Ticket Ticket { get; set; } = null!;
}
