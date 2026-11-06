using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Activos;

// Usado en POST y PUT. No exponer EmpresaId, EstadoActivoId ni auditoría en entradas.
public class ActivoGuardarDto
{
    [Required, MaxLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [MaxLength(100)] public string? NumeroInventario { get; set; }
    [MaxLength(150)] public string? NumeroSerie { get; set; }

    [Required, MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)] public string? Descripcion { get; set; }

    public Guid CategoriaActivoId { get; set; }
    public Guid? MarcaId { get; set; }
    public Guid? ModeloActivoId { get; set; }
    public Guid SucursalId { get; set; }
    public Guid? UbicacionId { get; set; }

    public Guid? ProveedorId { get; set; }

    [Required, MaxLength(20)]
    public string TipoPropiedad { get; set; } = "PROPIO";

    public Guid? PropietarioProveedorId { get; set; }
    [MaxLength(200)] public string? PropietarioNombre { get; set; }

    public DateTime? FechaAdquisicion { get; set; }
    public decimal? CostoAdquisicion { get; set; }

    [Required, StringLength(3, MinimumLength = 3)]
    public string Moneda { get; set; } = "MXN";

    [MaxLength(1000)] public string? Observaciones { get; set; }
}
