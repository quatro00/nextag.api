using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class Activo
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string Codigo { get; set; } = null!;

    public string? NumeroInventario { get; set; }

    public string? NumeroSerie { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public Guid CategoriaActivoId { get; set; }

    public Guid? MarcaId { get; set; }

    public Guid? ModeloActivoId { get; set; }

    public Guid SucursalId { get; set; }

    public Guid? UbicacionId { get; set; }

    public int EstadoActivoId { get; set; }

    public Guid? ProveedorId { get; set; }

    public string TipoPropiedad { get; set; } = null!;

    public Guid? PropietarioProveedorId { get; set; }

    public string? PropietarioNombre { get; set; }

    public DateTime? FechaAdquisicion { get; set; }

    public decimal? CostoAdquisicion { get; set; }

    public string Moneda { get; set; } = null!;

    public DateTime FechaAlta { get; set; }

    public DateTime? FechaBaja { get; set; }

    public string? Observaciones { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioModificacion { get; set; }

    public virtual ICollection<ActivoAsignacion> ActivoAsignacions { get; set; } = new List<ActivoAsignacion>();

    public virtual ICollection<ActivoEtiquetum> ActivoEtiqueta { get; set; } = new List<ActivoEtiquetum>();

    public virtual ICollection<ActivoMovimiento> ActivoMovimientos { get; set; } = new List<ActivoMovimiento>();

    public virtual CategoriaActivo CategoriaActivo { get; set; } = null!;

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual EstadoActivo EstadoActivo { get; set; } = null!;

    public virtual ICollection<EtiquetaLectura> EtiquetaLecturas { get; set; } = new List<EtiquetaLectura>();

    public virtual Marca? Marca { get; set; }

    public virtual ModeloActivo? ModeloActivo { get; set; }

    public virtual Proveedor? Proveedor { get; set; }

    public virtual Proveedor? ProveedorNavigation { get; set; }

    public virtual Sucursal Sucursal { get; set; } = null!;

    public virtual Ubicacion? Ubicacion { get; set; }
}
