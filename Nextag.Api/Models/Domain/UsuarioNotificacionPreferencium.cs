using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class UsuarioNotificacionPreferencium
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid UsuarioId { get; set; }

    public int NotificacionEventoId { get; set; }

    public string Canal { get; set; } = null!;

    public bool? Habilitado { get; set; }

    public bool ConsentimientoWhatsapp { get; set; }

    public DateTime? FechaConsentimiento { get; set; }

    public string? VersionAviso { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid? UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioModificacion { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual NotificacionEvento NotificacionEvento { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
