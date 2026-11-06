namespace Nextag.Api.Models.Dto.Etiquetas;

public class EtiquetaDto
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string TokenPublico { get; set; } = string.Empty;
    // El enlace apunta a una ruta pública que se implementará en una etapa posterior.
    public string? UrlPublica { get; set; }
    public string? UidNfc { get; set; }
    public bool TieneNfc { get; set; }
    public bool TieneQr { get; set; }
    public bool EsAntimetal { get; set; }
    public bool NfcBloqueado { get; set; }
    public bool Anulada { get; set; }
    public DateTime? FechaAnulacion { get; set; }
    public string? MotivoAnulacion { get; set; }
    public string Estatus { get; set; } = string.Empty; // DISPONIBLE, VINCULADA, RETIRADA
    public Guid? ActivoId { get; set; }
    public string? ActivoCodigo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public Guid UsuarioCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public Guid? UsuarioModificacion { get; set; }
}
