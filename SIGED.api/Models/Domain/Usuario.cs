using System;
using System.Collections.Generic;

namespace SIGED.api.Models.Domain;

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
}
