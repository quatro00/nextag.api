using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class Contrato
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string NumeroContrato { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public Guid TipoContratoId { get; set; }

    public int EstadoContratoId { get; set; }

    public Guid? ClienteId { get; set; }

    public Guid? ProveedorId { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public DateTime? FechaFirma { get; set; }

    public bool RenovacionAutomatica { get; set; }

    public int? DiasAvisoNoRenovacion { get; set; }

    public decimal? Monto { get; set; }

    public string Moneda { get; set; } = null!;

    public string? PeriodicidadPago { get; set; }

    public int? DiaPago { get; set; }

    public bool IncluyeImpuestos { get; set; }

    public bool? IncluyePreventivo { get; set; }

    public bool? IncluyeCorrectivo { get; set; }

    public bool? IncluyeRefacciones { get; set; }

    public bool? IncluyeManoObra { get; set; }

    public decimal? TiempoRespuestaHoras { get; set; }

    public string? HorarioAtencion { get; set; }

    public int? ServiciosIncluidos { get; set; }

    public string? CondicionesServicio { get; set; }

    public DateTime? FechaCancelacion { get; set; }

    public string? MotivoCancelacion { get; set; }

    public Guid? UsuarioCancelacion { get; set; }

    public DateTime? FechaFinalizacion { get; set; }

    public string? Observaciones { get; set; }

    public bool? Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioModificacion { get; set; }

    public virtual Cliente? Cliente { get; set; }

    public virtual ICollection<ContratoActivo> ContratoActivos { get; set; } = new List<ContratoActivo>();

    public virtual ICollection<ContratoRenovacion> ContratoRenovacions { get; set; } = new List<ContratoRenovacion>();

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual EstadoContrato EstadoContrato { get; set; } = null!;

    public virtual Proveedor? Proveedor { get; set; }

    public virtual TipoContrato TipoContrato { get; set; } = null!;
}
