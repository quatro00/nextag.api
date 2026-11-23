namespace Nextag.Api.Models.Dto.Archivos
{
    public class ArchivoRecuperacionResultadoDto
    {
        // ==========================================
        // FECHAS DE EJECUCION
        // ==========================================

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        // ==========================================
        // CONTADORES
        // ==========================================

        public int Encontrados { get; set; }

        public int Recuperados { get; set; }

        public int MarcadosError { get; set; }

        public int ReservasLiberadas { get; set; }

        public int ArchivosEliminados { get; set; }

        public int Fallidos { get; set; }

        public int Omitidos { get; set; }

        // ==========================================
        // OBSERVACIONES
        // ==========================================

        public List<string> Observaciones { get; set; }
            = new();
    }
}
