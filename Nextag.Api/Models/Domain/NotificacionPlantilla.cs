using System;
using System.Collections.Generic;

namespace Nextag.Api.Models.Domain;

public partial class NotificacionPlantilla
{
    public Guid Id { get; set; }

    public Guid? EmpresaId { get; set; }

    public int NotificacionEventoId { get; set; }

    public string Canal { get; set; } = null!;

    public string Idioma { get; set; } = null!;

    public string? Asunto { get; set; }

    public string Cuerpo { get; set; } = null!;

    public string? CodigoProveedor { get; set; }

    public bool? Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Guid? UsuarioCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public Guid? UsuarioModificacion { get; set; }

    public virtual Empresa? Empresa { get; set; }

    public virtual NotificacionEvento NotificacionEvento { get; set; } = null!;
}
