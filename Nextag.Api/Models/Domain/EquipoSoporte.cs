using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class EquipoSoporte
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string Clave { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string Tipo { get; set; } = null!;

    public Guid? ProveedorId { get; set; }

    public string? CorreoContacto { get; set; }

    public bool? Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioModificacion { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual ICollection<EquipoSoporteIntegrante> EquipoSoporteIntegrantes { get; set; } = new List<EquipoSoporteIntegrante>();

    public virtual ICollection<OrdenTrabajo> OrdenTrabajos { get; set; } = new List<OrdenTrabajo>();

    public virtual Proveedor? Proveedor { get; set; }

    public virtual ICollection<TicketAsignacion> TicketAsignacions { get; set; } = new List<TicketAsignacion>();
}
