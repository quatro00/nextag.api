namespace Nextag.Api.Models.Dto.Archivos
{
    public class ArchivoIntegridadResultadoDto
    {
        // ==========================================
        // RESULTADO
        // ==========================================

        public bool Valido { get; set; }

        public string Estado { get; set; }
            = string.Empty;

        public string? Mensaje { get; set; }

        // ==========================================
        // TAMAÑO
        // ==========================================

        public long TamanoEsperado { get; set; }

        public long? TamanoReal { get; set; }

        // ==========================================
        // HASH SHA256
        // ==========================================

        public string? HashEsperado { get; set; }

        public string? HashReal { get; set; }
    }
}
