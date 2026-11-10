using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class TicketFolioContador
{
    public Guid EmpresaId { get; set; }

    public long UltimoNumero { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;
}
