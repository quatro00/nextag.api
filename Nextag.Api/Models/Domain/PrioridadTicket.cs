using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class PrioridadTicket
{
    public int Id { get; set; }

    public string Clave { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public int Orden { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
