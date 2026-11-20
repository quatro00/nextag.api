using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class WhatsAppWebhookEvento
{
    public Guid Id { get; set; }

    public string ClaveIdempotencia { get; set; } = null!;

    public string WhatsAppBusinessAccountId { get; set; } = null!;

    public string PhoneNumberId { get; set; } = null!;

    public string IdentificadorProveedor { get; set; } = null!;

    public string EstadoMeta { get; set; } = null!;

    public DateTime? FechaEventoMeta { get; set; }

    public Guid? EmpresaId { get; set; }

    public Guid? NotificacionEnvioId { get; set; }

    public string PayloadJson { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public int Intentos { get; set; }

    public int MaxIntentos { get; set; }

    public DateTime? FechaProximoIntento { get; set; }

    public DateTime? FechaBloqueo { get; set; }

    public Guid? BloqueoToken { get; set; }

    public DateTime? FechaProcesamiento { get; set; }

    public string? UltimoError { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }
}
