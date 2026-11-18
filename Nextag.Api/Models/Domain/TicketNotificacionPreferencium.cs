using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class TicketNotificacionPreferencium
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid TicketId { get; set; }

    public bool RecibirCorreo { get; set; }

    public bool RecibirWhatsapp { get; set; }

    public bool ConsentimientoWhatsapp { get; set; }

    public DateTime? FechaConsentimiento { get; set; }

    public string? OrigenConsentimiento { get; set; }

    public string? VersionAviso { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid? UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioModificacion { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;
}
