using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class EstadoActivo
{
    public int Id { get; set; }

    public string Clave { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<ActivoMovimiento> ActivoMovimientoEstadoActivoDestinos { get; set; } = new List<ActivoMovimiento>();

    public virtual ICollection<ActivoMovimiento> ActivoMovimientoEstadoActivoOrigens { get; set; } = new List<ActivoMovimiento>();

    public virtual ICollection<Activo> Activos { get; set; } = new List<Activo>();
}
