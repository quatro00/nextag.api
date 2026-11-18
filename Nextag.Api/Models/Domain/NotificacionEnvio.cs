using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class NotificacionEnvio
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid NotificacionId { get; set; }

    public string Canal { get; set; } = null!;

    public string Destino { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public int Intentos { get; set; }

    public int MaxIntentos { get; set; }

    public DateTime? FechaProximoIntento { get; set; }

    public DateTime? FechaBloqueo { get; set; }

    public Guid? BloqueoToken { get; set; }

    public DateTime? FechaUltimoIntento { get; set; }

    public DateTime? FechaEnviado { get; set; }

    public string? Proveedor { get; set; }

    public string? IdentificadorProveedor { get; set; }

    public string? UltimoError { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public virtual Notificacion Notificacion { get; set; } = null!;
}
