using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class ContratoRenovacion
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid ContratoId { get; set; }

    public int NumeroRenovacion { get; set; }

    public DateTime FechaInicioAnterior { get; set; }

    public DateTime? FechaFinAnterior { get; set; }

    public DateTime FechaInicioNueva { get; set; }

    public DateTime? FechaFinNueva { get; set; }

    public decimal? MontoAnterior { get; set; }

    public decimal? MontoNuevo { get; set; }

    public string? Motivo { get; set; }

    public string? Observaciones { get; set; }

    public DateTime FechaRenovacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioModificacion { get; set; }

    public virtual Contrato Contrato { get; set; } = null!;

    public virtual Empresa Empresa { get; set; } = null!;
}
