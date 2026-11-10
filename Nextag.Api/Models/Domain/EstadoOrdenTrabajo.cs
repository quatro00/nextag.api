using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class EstadoOrdenTrabajo
{
    public int Id { get; set; }

    public string Clave { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public int Orden { get; set; }

    public bool EsFinal { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<OrdenTrabajo> OrdenTrabajos { get; set; } = new List<OrdenTrabajo>();
}
