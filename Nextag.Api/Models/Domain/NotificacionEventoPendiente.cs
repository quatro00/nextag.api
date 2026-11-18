using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class NotificacionEventoPendiente
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public int NotificacionEventoId { get; set; }

    public Guid? TicketId { get; set; }

    public string ClaveIdempotencia { get; set; } = null!;

    public string? PayloadJson { get; set; }

    public string Estado { get; set; } = null!;

    public int Intentos { get; set; }

    public int MaxIntentos { get; set; }

    public DateTime? FechaProximoIntento { get; set; }

    public DateTime? FechaBloqueo { get; set; }

    public Guid? BloqueoToken { get; set; }

    public DateTime? FechaProcesamiento { get; set; }

    public string? UltimoError { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid? UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual NotificacionEvento NotificacionEvento { get; set; } = null!;

    public virtual ICollection<Notificacion> Notificacions { get; set; } = new List<Notificacion>();

    public virtual Ticket? Ticket { get; set; }
}
