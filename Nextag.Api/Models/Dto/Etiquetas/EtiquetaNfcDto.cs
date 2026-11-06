using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Models.Dto.Etiquetas;

public class EtiquetaNfcDto
{
    [MaxLength(100)] public string? UidNfc { get; set; }
    public bool? NfcBloqueado { get; set; }
    // Declaración humana: el bloqueo se comprobó físicamente en el chip.
    // El backend NO puede bloquear un NFC mediante una actualización SQL.
    public bool ConfirmoVerificacionFisica { get; set; }
}
