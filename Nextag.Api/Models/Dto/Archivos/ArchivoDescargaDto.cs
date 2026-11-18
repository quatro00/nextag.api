namespace Nextag.Api.Models.Dto.Archivos
{
    public class ArchivoDescargaDto
    {
        public Stream Contenido { get; set; } = Stream.Null;

        public string NombreArchivo { get; set; } =
            string.Empty;

        public string MimeType { get; set; } =
            "application/octet-stream";
    }
}
