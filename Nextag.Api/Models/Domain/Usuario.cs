using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class Usuario
{
    public Guid Id { get; set; }

    public string AspNetUserId { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Celular { get; set; }

    public string? Pais { get; set; }

    public string? CodigoPais { get; set; }

    public string? Empresa { get; set; }

    public string? DominioPrincipal { get; set; }

    public bool? EsAdministrador { get; set; }

    public bool? Activo { get; set; }

    public DateTime? FechaUltimoAcceso { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public virtual AspNetUser AspNetUser { get; set; } = null!;

    public virtual ICollection<EquipoSoporteIntegrante> EquipoSoporteIntegrantes { get; set; } = new List<EquipoSoporteIntegrante>();

    public virtual ICollection<OrdenTrabajoActividad> OrdenTrabajoActividads { get; set; } = new List<OrdenTrabajoActividad>();

    public virtual ICollection<OrdenTrabajo> OrdenTrabajos { get; set; } = new List<OrdenTrabajo>();

    public virtual ICollection<TicketAsignacion> TicketAsignacions { get; set; } = new List<TicketAsignacion>();

    public virtual ICollection<TicketSeguimiento> TicketSeguimientos { get; set; } = new List<TicketSeguimiento>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual ICollection<UsuarioEmpresa> UsuarioEmpresas { get; set; } = new List<UsuarioEmpresa>();
}
