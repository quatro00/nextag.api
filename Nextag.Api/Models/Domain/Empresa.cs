using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class Empresa
{
    public Guid Id { get; set; }

    public string Clave { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? RazonSocial { get; set; }

    public string? Rfc { get; set; }

    public string? Correo { get; set; }

    public string? Telefono { get; set; }

    public string? SitioWeb { get; set; }

    public string? LogoUrl { get; set; }

    public bool? Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioModificacion { get; set; }

    public virtual ICollection<ActivoAsignacion> ActivoAsignacions { get; set; } = new List<ActivoAsignacion>();

    public virtual ICollection<ActivoMovimiento> ActivoMovimientos { get; set; } = new List<ActivoMovimiento>();

    public virtual ICollection<Activo> Activos { get; set; } = new List<Activo>();

    public virtual ICollection<ArchivoAdjunto> ArchivoAdjuntos { get; set; } = new List<ArchivoAdjunto>();

    public virtual ICollection<CategoriaActivo> CategoriaActivos { get; set; } = new List<CategoriaActivo>();

    public virtual ICollection<CategoriaTicket> CategoriaTickets { get; set; } = new List<CategoriaTicket>();

    public virtual ICollection<EquipoSoporte> EquipoSoportes { get; set; } = new List<EquipoSoporte>();

    public virtual ICollection<Etiquetum> Etiqueta { get; set; } = new List<Etiquetum>();

    public virtual EtiquetaConfiguracion? EtiquetaConfiguracion { get; set; }

    public virtual ICollection<Marca> Marcas { get; set; } = new List<Marca>();

    public virtual ICollection<ModeloActivo> ModeloActivos { get; set; } = new List<ModeloActivo>();

    public virtual ICollection<OrdenTrabajo> OrdenTrabajos { get; set; } = new List<OrdenTrabajo>();

    public virtual ICollection<Proveedor> Proveedors { get; set; } = new List<Proveedor>();

    public virtual ICollection<Responsable> Responsables { get; set; } = new List<Responsable>();

    public virtual ICollection<Sucursal> Sucursals { get; set; } = new List<Sucursal>();

    public virtual TicketFolioContador? TicketFolioContador { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual ICollection<TipoUbicacion> TipoUbicacions { get; set; } = new List<TipoUbicacion>();

    public virtual ICollection<Ubicacion> Ubicacions { get; set; } = new List<Ubicacion>();

    public virtual ICollection<UsuarioEmpresa> UsuarioEmpresas { get; set; } = new List<UsuarioEmpresa>();
}
