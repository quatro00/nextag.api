using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class ActivoMovimiento
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid ActivoId { get; set; }

    public int TipoMovimientoActivoId { get; set; }

    public DateTime FechaMovimiento { get; set; }

    public string? Motivo { get; set; }

    public string? Observaciones { get; set; }

    public Guid? SucursalOrigenId { get; set; }

    public Guid? UbicacionOrigenId { get; set; }

    public string? SucursalOrigenNombre { get; set; }

    public string? UbicacionOrigenNombre { get; set; }

    public bool EsOrigenExterno { get; set; }

    public string? OrigenExternoNombre { get; set; }

    public string? OrigenExternoDireccion { get; set; }

    public Guid? SucursalDestinoId { get; set; }

    public Guid? UbicacionDestinoId { get; set; }

    public string? SucursalDestinoNombre { get; set; }

    public string? UbicacionDestinoNombre { get; set; }

    public bool EsDestinoExterno { get; set; }

    public string? DestinoExternoNombre { get; set; }

    public string? DestinoExternoDireccion { get; set; }

    public Guid? ProveedorDestinoId { get; set; }

    public Guid? MovimientoOrigenId { get; set; }

    public int EstadoActivoOrigenId { get; set; }

    public int EstadoActivoDestinoId { get; set; }

    public string? TipoReferencia { get; set; }

    public Guid? ReferenciaId { get; set; }

    public bool Anulado { get; set; }

    public DateTime? FechaAnulacion { get; set; }

    public Guid? UsuarioAnulacion { get; set; }

    public string? MotivoAnulacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioModificacion { get; set; }

    public virtual Activo Activo { get; set; } = null!;

    public virtual ActivoMovimiento? ActivoMovimientoNavigation { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual EstadoActivo EstadoActivoDestino { get; set; } = null!;

    public virtual EstadoActivo EstadoActivoOrigen { get; set; } = null!;

    public virtual ICollection<ActivoMovimiento> InverseActivoMovimientoNavigation { get; set; } = new List<ActivoMovimiento>();

    public virtual Proveedor? Proveedor { get; set; }

    public virtual Sucursal? Sucursal { get; set; }

    public virtual Sucursal? SucursalNavigation { get; set; }

    public virtual TipoMovimientoActivo TipoMovimientoActivo { get; set; } = null!;

    public virtual Ubicacion? Ubicacion { get; set; }

    public virtual Ubicacion? UbicacionNavigation { get; set; }
}
