using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class NotificacionEvento
{
    public int Id { get; set; }

    public string Clave { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool? Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<NotificacionEventoPendiente> NotificacionEventoPendientes { get; set; } = new List<NotificacionEventoPendiente>();

    public virtual ICollection<NotificacionPlantilla> NotificacionPlantillas { get; set; } = new List<NotificacionPlantilla>();

    public virtual ICollection<UsuarioNotificacionPreferencium> UsuarioNotificacionPreferencia { get; set; } = new List<UsuarioNotificacionPreferencium>();
}
