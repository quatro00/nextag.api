using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class Etiquetum
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string Codigo { get; set; } = null!;

    public string TokenPublico { get; set; } = null!;

    public string? UidNfc { get; set; }

    public bool? TieneNfc { get; set; }

    public bool? TieneQr { get; set; }

    public bool EsAntimetal { get; set; }

    public bool NfcBloqueado { get; set; }

    public bool Anulada { get; set; }

    public DateTime? FechaAnulacion { get; set; }

    public Guid? UsuarioAnulacion { get; set; }

    public string? MotivoAnulacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioModificacion { get; set; }

    public bool? PublicaHabilitada { get; set; }

    public virtual ActivoEtiquetum? ActivoEtiquetum { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual ICollection<EtiquetaLectura> EtiquetaLecturas { get; set; } = new List<EtiquetaLectura>();
}
