using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nextag.Api.Data;
using Nextag.Api.Models.Dto.Archivos;
using Nextag.Api.Services.Archivos.Interface;
using System.Security.Cryptography;

namespace Nextag.Api.Services.Archivos.Implementation
{
    public class ArchivoAdjuntoRecuperacionService
        : IArchivoAdjuntoRecuperacionService
    {
        // ==========================================
        // DEPENDENCIAS
        // ==========================================

        private readonly NextagContext context;

        private readonly IArchivoStorageService storage;

        private readonly ILogger<ArchivoAdjuntoRecuperacionService>
            logger;

        // ==========================================
        // CONFIGURACION
        // ==========================================

        private const int MinutosAntiguedad = 30;

        private const int CantidadMaxima = 100;

        // ==========================================
        // CONSTRUCTOR
        // ==========================================

        public ArchivoAdjuntoRecuperacionService(
            NextagContext context,
            IArchivoStorageService storage,
            ILogger<ArchivoAdjuntoRecuperacionService> logger)
        {
            this.context = context;

            this.storage = storage;

            this.logger = logger;
        }

        // ==========================================
        // VALIDAR INTEGRIDAD DEL ARCHIVO
        // ==========================================

        private async Task<ArchivoIntegridadResultadoDto>
            ValidarIntegridadAsync(
                string rutaStorage,
                long tamanoEsperado,
                string? hashEsperado,
                CancellationToken ct)
        {
            var resultado = new ArchivoIntegridadResultadoDto
            {
                Valido = false,

                Estado = "PENDIENTE_VALIDACION",

                TamanoEsperado = tamanoEsperado,

                HashEsperado = hashEsperado
            };

            // ======================================
            // VALIDAR METADATOS
            // ======================================

            if (
                string.IsNullOrWhiteSpace(rutaStorage) ||
                tamanoEsperado <= 0 ||
                string.IsNullOrWhiteSpace(hashEsperado) ||
                hashEsperado.Length != 64 ||
                !hashEsperado.All(Uri.IsHexDigit)
            )
            {
                resultado.Estado = "METADATOS_INVALIDOS";

                resultado.Mensaje =
                    "Los metadatos del archivo son inválidos o están incompletos.";

                return resultado;
            }

            // ======================================
            // VERIFICAR EXISTENCIA
            // ======================================

            try
            {
                var existe = await storage.ExisteAsync(
                    rutaStorage,
                    ct);

                if (!existe)
                {
                    resultado.Estado = "NO_EXISTE";

                    resultado.Mensaje =
                        "El archivo físico no existe.";

                    return resultado;
                }

                // ==================================
                // ABRIR ARCHIVO
                // ==================================

                await using var stream =
                    await storage.AbrirLecturaAsync(
                        rutaStorage,
                        ct);

                // ==================================
                // CALCULAR HASH Y TAMAÑO
                // ==================================

                using var sha256 =
                    IncrementalHash.CreateHash(
                        HashAlgorithmName.SHA256);

                var buffer = new byte[81920];

                long totalBytes = 0;

                int bytesLeidos;

                while (
                    (bytesLeidos = await stream.ReadAsync(
                        buffer.AsMemory(),
                        ct)) > 0
                )
                {
                    totalBytes += bytesLeidos;

                    // ==================================
                    // PROTEGER CONTRA TAMAÑOS INESPERADOS
                    // ==================================

                    if (totalBytes > tamanoEsperado)
                    {
                        resultado.TamanoReal = totalBytes;

                        resultado.Estado = "TAMANO_INVALIDO";

                        resultado.Mensaje =
                            "El archivo físico supera el tamaño registrado.";

                        return resultado;
                    }

                    // ==================================
                    // INCORPORAR CONTENIDO AL HASH
                    // ==================================

                    sha256.AppendData(
                        buffer,
                        0,
                        bytesLeidos);
                }

                resultado.TamanoReal = totalBytes;

                // ==================================
                // COMPARAR TAMAÑO
                // ==================================

                if (totalBytes != tamanoEsperado)
                {
                    resultado.Estado = "TAMANO_INVALIDO";

                    resultado.Mensaje =
                        "El tamaño físico no coincide con el registrado.";

                    return resultado;
                }

                // ==================================
                // OBTENER SHA256
                // ==================================

                var hashReal = Convert
                    .ToHexString(
                        sha256.GetHashAndReset())
                    .ToLowerInvariant();

                resultado.HashReal = hashReal;

                // ==================================
                // COMPARAR HASH
                // ==================================

                if (!string.Equals(
                    hashReal,
                    hashEsperado,
                    StringComparison.OrdinalIgnoreCase))
                {
                    resultado.Estado = "HASH_INVALIDO";

                    resultado.Mensaje =
                        "El contenido del archivo no coincide con el hash registrado.";

                    return resultado;
                }

                // ==================================
                // ARCHIVO VALIDO
                // ==================================

                resultado.Valido = true;

                resultado.Estado = "VALIDO";

                resultado.Mensaje =
                    "El archivo existe y su integridad fue verificada.";

                return resultado;
            }
            catch (OperationCanceledException)
                when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (FileNotFoundException)
            {
                resultado.Estado = "NO_EXISTE";

                resultado.Mensaje =
                    "El archivo desapareció durante la validación.";

                return resultado;
            }
            catch (DirectoryNotFoundException)
            {
                resultado.Estado = "NO_EXISTE";

                resultado.Mensaje =
                    "El directorio del archivo no existe.";

                return resultado;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error al validar integridad del archivo {RutaStorage}",
                    rutaStorage);

                resultado.Estado = "ERROR_LECTURA";

                resultado.Mensaje =
                    "No fue posible leer o validar el archivo.";

                return resultado;
            }
        }

        // ==========================================
        // PROCESAR ARCHIVOS PENDIENTES
        // ==========================================

        public async Task<ArchivoRecuperacionResultadoDto>
            ProcesarPendientesAsync(
                int cantidad = 20,
                CancellationToken ct = default)
        {
            // ======================================
            // NORMALIZAR CANTIDAD
            // ======================================

            cantidad = Math.Clamp(
                cantidad,
                1,
                CantidadMaxima);

            var resultado =
                new ArchivoRecuperacionResultadoDto
                {
                    FechaInicio = DateTime.UtcNow
                };

            try
            {
                // ==================================
                // CALCULAR ANTIGUEDAD
                // ==================================

                var ahora = DateTime.UtcNow;

                var fechaLimite =
                    ahora.AddMinutes(
                        -MinutosAntiguedad);

                // ==================================
                // CONSULTAR ARCHIVOS
                // ==================================

                var candidatos = await context
                    .ArchivoAdjuntos
                    .AsNoTracking()
                    .Where(x =>

                        x.EstadoAlmacenamiento ==
                            "PENDIENTE" &&

                        x.Eliminado == false &&

                        x.FechaCreacion <= fechaLimite

                    )
                    .OrderBy(x => x.FechaCreacion)
                    .ThenBy(x => x.Id)
                    .Select(x => new
                    {
                        x.Id,

                        x.EmpresaId,

                        x.TicketId,

                        x.TicketAdjuntoTokenId,

                        x.RutaStorage,

                        x.TamanoBytes,

                        x.HashSha256,

                        x.FechaCreacion
                    })
                    .Take(cantidad)
                    .ToListAsync(ct);

                // ==================================
                // REGISTRAR CANTIDAD
                // ==================================

                resultado.Encontrados =
                    candidatos.Count;

                logger.LogInformation(
                    "Recuperación de adjuntos: " +
                    "{Cantidad} archivos pendientes encontrados.",
                    candidatos.Count);

                // ==================================
                // EVALUAR CANDIDATOS
                // ==================================

                foreach (var archivo in candidatos)
                {
                    ct.ThrowIfCancellationRequested();

                    try
                    {
                        // ==========================
                        // VALIDAR DATOS BASICOS
                        // ==========================

                        if (archivo.TicketId == null)
                        {
                            resultado.Omitidos++;

                            logger.LogWarning(
                                "Archivo {ArchivoId} " +
                                "sin TicketId. Empresa: {EmpresaId}",
                                archivo.Id,
                                archivo.EmpresaId);

                            continue;
                        }

                        // ==========================================
                        // VALIDAR INTEGRIDAD
                        // ==========================================

                        var integridad = await ValidarIntegridadAsync(
                            archivo.RutaStorage,
                            archivo.TamanoBytes,
                            archivo.HashSha256,
                            ct);

                        // ==========================================
                        // REGISTRAR RESULTADO
                        // ==========================================

                        if (integridad.Valido)
                        {
                            resultado.ArchivosValidos++;
                            logger.LogInformation(
                                "Archivo {ArchivoId} válido. " +
                                "Empresa: {EmpresaId}. " +
                                "Tamaño: {TamanoBytes} bytes.",
                                archivo.Id,
                                archivo.EmpresaId,
                                integridad.TamanoReal);
                        }
                        else
                        {
                            resultado.ArchivosInvalidos++;
                            logger.LogWarning(
                                "Archivo {ArchivoId} no válido. " +
                                "Empresa: {EmpresaId}. " +
                                "Estado: {Estado}. " +
                                "Mensaje: {Mensaje}.",
                                archivo.Id,
                                archivo.EmpresaId,
                                integridad.Estado,
                                integridad.Mensaje);
                        }

                        // ==========================================
                        // SIGUIENTES ETAPAS
                        // ==========================================
                        //
                        // 7H-G:
                        // Adquirir archivo mediante transición
                        // atómica y modificar su estado.
                        //
                        // 7H-H:
                        // Liberar reservas públicas cuando
                        // corresponda.
                        //
                        // 7H-I:
                        // Limpiar archivos físicos residuales.
                        //
                        // 7H-F únicamente valida integridad.

                    }
                    catch (OperationCanceledException)
                        when (ct.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        resultado.Fallidos++;

                        logger.LogError(
                            ex,
                            "Error al evaluar archivo pendiente " +
                            "{ArchivoId}. Empresa: {EmpresaId}",
                            archivo.Id,
                            archivo.EmpresaId);
                    }
                }
            }
            finally
            {
                resultado.FechaFin =
                    DateTime.UtcNow;

                logger.LogInformation(
                    "Evaluación de recuperación finalizada. " +
                    "Encontrados: {Encontrados}, " +
                    "Fallidos: {Fallidos}, " +
                    "Omitidos: {Omitidos}",
                    resultado.Encontrados,
                    resultado.Fallidos,
                    resultado.Omitidos);
            }

            return resultado;
        }
    }
}
