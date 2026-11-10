using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class TicketSeguimiento
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid TicketId { get; set; }

    public string Tipo { get; set; } = null!;

    public string Comentario { get; set; } = null!;

    public bool VisibleSolicitante { get; set; }

    public Guid? AutorUsuarioId { get; set; }

    public string? AutorNombre { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<ArchivoAdjunto> ArchivoAdjuntos { get; set; } = new List<ArchivoAdjunto>();

    public virtual Usuario? AutorUsuario { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;
}
