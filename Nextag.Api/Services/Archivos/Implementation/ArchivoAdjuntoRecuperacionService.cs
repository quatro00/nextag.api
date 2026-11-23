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
        // CONTROL DE RECUPERACION
        // ==========================================

        // Una recuperación que no se actualiza durante
        // este tiempo puede ser adquirida nuevamente.
        private const int MinutosBloqueo = 15;

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
        // MARCAR ARCHIVO PUBLICO COMO ERROR
        // Y LIBERAR RESERVA
        // ==========================================

        private async Task<bool> MarcarErrorPublicoAsync(
            Guid archivoId,
            Guid empresaId,
            Guid recuperacionId,
            Guid autorizacionId,
            CancellationToken ct)
        {
            var ahora = DateTime.UtcNow;

            await using var transaction =
                await context.Database.BeginTransactionAsync(ct);

            // ======================================
            // VALIDAR Y ACTUALIZAR ARCHIVO
            // ======================================

            var filasArchivo = await context.ArchivoAdjuntos
                .Where(x =>
                    x.Id == archivoId &&
                    x.EmpresaId == empresaId &&
                    !x.Eliminado &&
                    x.TicketAdjuntoTokenId == autorizacionId &&
                    x.EstadoAlmacenamiento == "RECUPERANDO" &&
                    x.RecuperacionId == recuperacionId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        x => x.EstadoAlmacenamiento,
                        "ERROR")
                    .SetProperty(
                        x => x.RecuperacionId,
                        (Guid?)null)
                    .SetProperty(
                        x => x.FechaActualizacion,
                        ahora),
                    ct);

            if (filasArchivo != 1)
            {
                await transaction.RollbackAsync(ct);

                return false;
            }

            // ======================================
            // LIBERAR RESERVA PUBLICA
            // ======================================

            var filasToken = await context.TicketAdjuntoTokens
                .Where(x =>
                    x.Id == autorizacionId &&
                    x.EmpresaId == empresaId &&
                    x.ArchivosCargados > 0)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        x => x.ArchivosCargados,
                        x => x.ArchivosCargados - 1),
                    ct);

            if (filasToken != 1)
            {
                await transaction.RollbackAsync(ct);

                logger.LogWarning(
                    "No fue posible liberar la reserva del archivo {ArchivoId}. " +
                    "Autorización: {AutorizacionId}",
                    archivoId,
                    autorizacionId);

                return false;
            }

            // ======================================
            // CONFIRMAR TRANSACCION
            // ======================================

            await transaction.CommitAsync(ct);

            logger.LogInformation(
                "Archivo público {ArchivoId} marcado como ERROR. " +
                "Reserva {AutorizacionId} liberada.",
                archivoId,
                autorizacionId);

            return true;
        }

        // ==========================================
        // ADQUIRIR ARCHIVO PARA RECUPERACION
        // ==========================================

        private async Task<bool> AdquirirArchivoAsync(
            Guid archivoId,
            Guid empresaId,
            Guid recuperacionId,
            CancellationToken ct)
        {
            var ahora = DateTime.UtcNow;

            var fechaPendiente = ahora.AddMinutes(
                -MinutosAntiguedad);

            var fechaBloqueo = ahora.AddMinutes(
                -MinutosBloqueo);

            var filas = await context.ArchivoAdjuntos
                .Where(x =>
                    x.Id == archivoId &&
                    x.EmpresaId == empresaId &&
                    !x.Eliminado &&
                    (
                        (
                            x.EstadoAlmacenamiento == "PENDIENTE" &&
                            x.FechaCreacion <= fechaPendiente &&
                            (
                                x.FechaActualizacion == null ||
                                x.FechaActualizacion <= fechaPendiente
                            )
                        )
                        ||
                        (
                            x.EstadoAlmacenamiento == "RECUPERANDO" &&
                            x.FechaActualizacion != null &&
                            x.FechaActualizacion <= fechaBloqueo
                        )
                    )
                )
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        x => x.EstadoAlmacenamiento,
                        "RECUPERANDO")
                    .SetProperty(
                        x => x.RecuperacionId,
                        recuperacionId)
                    .SetProperty(
                        x => x.FechaActualizacion,
                        ahora),
                    ct);

            return filas == 1;
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
        // MARCAR ARCHIVO INTERNO COMO ERROR
        // ==========================================

        private async Task<bool> MarcarErrorInternoAsync(
            Guid archivoId,
            Guid empresaId,
            Guid recuperacionId,
            CancellationToken ct)
        {
            var ahora = DateTime.UtcNow;

            var filas = await context.ArchivoAdjuntos
                .Where(x =>
                    x.Id == archivoId &&
                    x.EmpresaId == empresaId &&
                    !x.Eliminado &&
                    x.TicketAdjuntoTokenId == null &&
                    x.EstadoAlmacenamiento == "RECUPERANDO" &&
                    x.RecuperacionId == recuperacionId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        x => x.EstadoAlmacenamiento,
                        "ERROR")
                    .SetProperty(
                        x => x.RecuperacionId,
                        (Guid?)null)
                    .SetProperty(
                        x => x.FechaActualizacion,
                        ahora),
                    ct);

            return filas == 1;
        }

        // ==========================================
        // LIBERAR ARCHIVO PARA REINTENTO
        // ==========================================

        private async Task<bool> LiberarParaReintentoAsync(
            Guid archivoId,
            Guid empresaId,
            Guid recuperacionId,
            CancellationToken ct)
        {
            var ahora = DateTime.UtcNow;

            var filas = await context.ArchivoAdjuntos
                .Where(x =>
                    x.Id == archivoId &&
                    x.EmpresaId == empresaId &&
                    !x.Eliminado &&
                    x.EstadoAlmacenamiento == "RECUPERANDO" &&
                    x.RecuperacionId == recuperacionId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        x => x.EstadoAlmacenamiento,
                        "PENDIENTE")
                    .SetProperty(
                        x => x.RecuperacionId,
                        (Guid?)null)
                    .SetProperty(
                        x => x.FechaActualizacion,
                        ahora),
                    ct);

            return filas == 1;
        }

        // ==========================================
        // CONFIRMAR ARCHIVO RECUPERADO
        // ==========================================

        private async Task<bool> ConfirmarRecuperacionAsync(
            Guid archivoId,
            Guid empresaId,
            Guid recuperacionId,
            CancellationToken ct)
        {
            var ahora = DateTime.UtcNow;

            var filas = await context.ArchivoAdjuntos
                .Where(x =>
                    x.Id == archivoId &&
                    x.EmpresaId == empresaId &&
                    !x.Eliminado &&
                    x.EstadoAlmacenamiento == "RECUPERANDO" &&
                    x.RecuperacionId == recuperacionId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        x => x.EstadoAlmacenamiento,
                        "DISPONIBLE")
                    .SetProperty(
                        x => x.RecuperacionId,
                        (Guid?)null)
                    .SetProperty(
                        x => x.FechaActualizacion,
                        ahora),
                    ct);

            return filas == 1;
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

                var fechaBloqueo = ahora.AddMinutes(-MinutosBloqueo);

                // ==================================
                // CONSULTAR ARCHIVOS
                // ==================================

                var candidatos = await context
                    .ArchivoAdjuntos
                    .AsNoTracking()
                    .Where(x =>
                        !x.Eliminado &&
                        (
                            (
                                x.EstadoAlmacenamiento == "PENDIENTE" &&
                                x.FechaCreacion <= fechaLimite &&
                                (
                                    x.FechaActualizacion == null ||
                                    x.FechaActualizacion <= fechaLimite
                                )
                            )
                            ||
                            (
                                x.EstadoAlmacenamiento == "RECUPERANDO" &&
                                x.FechaActualizacion != null &&
                                x.FechaActualizacion <= fechaBloqueo
                            )
                        )
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

                    // ======================================
                    // GENERAR IDENTIFICADOR DE RECUPERACION
                    // ======================================

                    var recuperacionId = Guid.NewGuid();

                    var adquirido = false;

                    var finalizado = false;

                    try
                    {
                        // ==================================
                        // VALIDAR DATOS BASICOS
                        // ==================================

                        if (archivo.TicketId == null)
                        {
                            resultado.Omitidos++;

                            logger.LogWarning(
                                "Archivo {ArchivoId} sin TicketId. Empresa: {EmpresaId}",
                                archivo.Id,
                                archivo.EmpresaId);

                            continue;
                        }

                        // ==================================
                        // ADQUIRIR ARCHIVO
                        // ==================================

                        adquirido = await AdquirirArchivoAsync(
                            archivo.Id,
                            archivo.EmpresaId,
                            recuperacionId,
                            ct);

                        if (!adquirido)
                        {
                            resultado.Omitidos++;

                            logger.LogInformation(
                                "El archivo {ArchivoId} ya está siendo procesado.",
                                archivo.Id);

                            continue;
                        }

                        // ==================================
                        // VALIDAR INTEGRIDAD
                        // ==================================

                        var integridad = await ValidarIntegridadAsync(
                            archivo.RutaStorage,
                            archivo.TamanoBytes,
                            archivo.HashSha256,
                            ct);

                        // ==================================
                        // ARCHIVO VALIDO
                        // ==================================

                        if (integridad.Valido)
                        {
                            resultado.ArchivosValidos++;

                            var confirmado = await ConfirmarRecuperacionAsync(
                                archivo.Id,
                                archivo.EmpresaId,
                                recuperacionId,
                                ct);

                            if (confirmado)
                            {
                                resultado.Recuperados++;

                                finalizado = true;

                                logger.LogInformation(
                                    "Archivo {ArchivoId} recuperado correctamente.",
                                    archivo.Id);
                            }
                            else
                            {
                                resultado.Fallidos++;

                                logger.LogWarning(
                                    "No fue posible confirmar la recuperación " +
                                    "del archivo {ArchivoId}.",
                                    archivo.Id);
                            }

                            continue;
                        }

                        // ==================================
                        // ARCHIVO NO VALIDO
                        // ==================================

                        resultado.ArchivosInvalidos++;

                        logger.LogWarning(
                            "Archivo {ArchivoId} no válido. " +
                            "Estado: {Estado}. Mensaje: {Mensaje}",
                            archivo.Id,
                            integridad.Estado,
                            integridad.Mensaje);

                        // ==================================
                        // ERRORES TEMPORALES
                        // ==================================

                        if (integridad.Estado == "ERROR_LECTURA")
                        {
                            finalizado = await LiberarParaReintentoAsync(
                                archivo.Id,
                                archivo.EmpresaId,
                                recuperacionId,
                                ct);

                            if (!finalizado)
                            {
                                resultado.Fallidos++;
                            }

                            continue;
                        }

                        // ==========================================
                        // ARCHIVOS PUBLICOS
                        // ==========================================

                        if (archivo.TicketAdjuntoTokenId.HasValue)
                        {
                            var marcadoPublico = await MarcarErrorPublicoAsync(
                                archivo.Id,
                                archivo.EmpresaId,
                                recuperacionId,
                                archivo.TicketAdjuntoTokenId.Value,
                                ct);

                            if (marcadoPublico)
                            {
                                resultado.MarcadosError++;
                                resultado.ReservasLiberadas++;

                                finalizado = true;

                                logger.LogInformation(
                                    "Archivo público {ArchivoId} marcado como ERROR. " +
                                    "Reserva liberada correctamente.",
                                    archivo.Id);
                            }
                            else
                            {
                                resultado.Fallidos++;

                                logger.LogWarning(
                                    "No fue posible conciliar la reserva " +
                                    "del archivo público {ArchivoId}.",
                                    archivo.Id);
                            }

                            continue;
                        }

                        // ==================================
                        // ARCHIVOS INTERNOS
                        // ==================================

                        var marcado = await MarcarErrorInternoAsync(
                            archivo.Id,
                            archivo.EmpresaId,
                            recuperacionId,
                            ct);

                        if (marcado)
                        {
                            resultado.MarcadosError++;

                            finalizado = true;

                            logger.LogWarning(
                                "Archivo interno {ArchivoId} marcado como ERROR.",
                                archivo.Id);
                        }
                        else
                        {
                            resultado.Fallidos++;

                            logger.LogWarning(
                                "No fue posible marcar como ERROR " +
                                "el archivo {ArchivoId}.",
                                archivo.Id);
                        }
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
                            "Error al recuperar archivo {ArchivoId}. Empresa: {EmpresaId}",
                            archivo.Id,
                            archivo.EmpresaId);
                    }
                    finally
                    {
                        // ==================================
                        // LIBERAR BLOQUEO SI QUEDO ACTIVO
                        // ==================================

                        if (adquirido && !finalizado)
                        {
                            try
                            {
                                // La recuperación puede ocurrir después
                                // de que se canceló la solicitud.
                                // Utilizamos un token independiente
                                // para intentar liberar el bloqueo.

                                using var recuperacionCts =
                                    new CancellationTokenSource(
                                        TimeSpan.FromSeconds(15));

                                var liberado = await LiberarParaReintentoAsync(
                                    archivo.Id,
                                    archivo.EmpresaId,
                                    recuperacionId,
                                    recuperacionCts.Token);

                                if (!liberado)
                                {
                                    logger.LogWarning(
                                        "No se pudo liberar la recuperación " +
                                        "del archivo {ArchivoId}. " +
                                        "Se reintentará cuando expire el bloqueo.",
                                        archivo.Id);
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(
                                    ex,
                                    "Error al liberar bloqueo del archivo {ArchivoId}.",
                                    archivo.Id);
                            }
                        }
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
