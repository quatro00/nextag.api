namespace Nextag.Api.Models.Configuration
{
    public class ArchivoStorageOptions
    {
        public const string SectionName = "ArchivoStorage";

        public string Provider { get; set; } = "Local";

        public string RutaLocal { get; set; } = "../NextagStorage";

        public long MaximoBytesArchivo { get; set; } =
            10 * 1024 * 1024;
    }
}
