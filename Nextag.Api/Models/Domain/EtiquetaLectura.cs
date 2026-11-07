using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class EtiquetaLectura
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid EtiquetaId { get; set; }

    public Guid? ActivoId { get; set; }

    public string TipoLectura { get; set; } = null!;

    public DateTime FechaLectura { get; set; }

    public string? Ip { get; set; }

    public string? UserAgent { get; set; }

    public string Resultado { get; set; } = null!;

    public virtual Activo? Activo { get; set; }

    public virtual Etiquetum E { get; set; } = null!;
}
