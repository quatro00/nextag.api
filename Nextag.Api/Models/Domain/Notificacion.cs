using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class Notificacion
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid EventoPendienteId { get; set; }

    public Guid? TicketId { get; set; }

    public string TipoDestinatario { get; set; } = null!;

    public string DestinatarioClave { get; set; } = null!;

    public Guid? UsuarioId { get; set; }

    public string? DestinatarioNombre { get; set; }

    public string? DestinatarioCorreo { get; set; }

    public string? DestinatarioTelefono { get; set; }

    public bool MostrarInterna { get; set; }

    public string Titulo { get; set; } = null!;

    public string Mensaje { get; set; } = null!;

    public string? RutaNavegacion { get; set; }

    public DateTime? FechaLectura { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid? UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public virtual NotificacionEventoPendiente E { get; set; } = null!;

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual ICollection<NotificacionEnvio> NotificacionEnvios { get; set; } = new List<NotificacionEnvio>();

    public virtual Ticket? Ticket { get; set; }

    public virtual Usuario? Usuario { get; set; }
}
