namespace Nextag.Api.Models.Dto.Etiquetas;

public class ActivoEtiquetaHistorialDto
{
    public Guid Id { get; set; } // Id de la asociación histórica
    public Guid ActivoId { get; set; }
    public Guid EtiquetaId { get; set; }
    public string EtiquetaCodigo { get; set; } = string.Empty;
    public string TokenPublico { get; set; } = string.Empty;
    public string? UrlPublica { get; set; }
    public bool TieneNfc { get; set; }
    public bool TieneQr { get; set; }
    public bool EsAntimetal { get; set; }
    public bool NfcBloqueado { get; set; }
    public bool Anulada { get; set; }
    public bool Vigente { get; set; }
    public DateTime FechaVinculacion { get; set; }
    public Guid UsuarioVinculacion { get; set; }
    public DateTime? FechaDesvinculacion { get; set; }
    public Guid? UsuarioDesvinculacion { get; set; }
    public string? MotivoDesvinculacion { get; set; }
}
