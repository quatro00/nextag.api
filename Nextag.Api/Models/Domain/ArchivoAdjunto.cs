using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class ArchivoAdjunto
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid? TicketId { get; set; }

    public Guid? TicketSeguimientoId { get; set; }

    public Guid? OrdenTrabajoId { get; set; }

    public string NombreOriginal { get; set; } = null!;

    public string NombreAlmacenado { get; set; } = null!;

    public string MimeType { get; set; } = null!;

    public long TamanoBytes { get; set; }

    public string RutaStorage { get; set; } = null!;

    public string? HashSha256 { get; set; }

    public bool VisibleSolicitante { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid? UsuarioCreacion { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual OrdenTrabajo? OrdenTrabajo { get; set; }

    public virtual Ticket? Ticket { get; set; }

    public virtual TicketSeguimiento? TicketSeguimiento { get; set; }
}
