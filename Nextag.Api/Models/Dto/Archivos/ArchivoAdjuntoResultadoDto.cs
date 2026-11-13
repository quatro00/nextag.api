namespace Nextag.Api.Models.Dto.Archivos
{
    public class ArchivoAdjuntoResultadoDto
    {
        public bool Success { get; set; }

        public string Message { get; set; } =
            string.Empty;

        public ArchivoAdjuntoDto? Archivo { get; set; }

        public static ArchivoAdjuntoResultadoDto Ok(
            ArchivoAdjuntoDto archivo)
        {
            return new ArchivoAdjuntoResultadoDto
            {
                Success = true,
                Message = "Archivo adjuntado correctamente.",
                Archivo = archivo
            };
        }

        public static ArchivoAdjuntoResultadoDto Fail(
            string mensaje)
        {
            return new ArchivoAdjuntoResultadoDto
            {
                Success = false,
                Message = mensaje
            };
        }
    }
}
