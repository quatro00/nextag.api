using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class EstadoContrato
{
    public int Id { get; set; }

    public string Clave { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool? Activo { get; set; }

    public int Orden { get; set; }

    public virtual ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
}
