using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nextag.Api.Data;
using Nextag.Api.Models.Configuration;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto.Archivos;
using Nextag.Api.Services.Archivos.Interface;
using Nextag.Api.Services.Tickets.Interface;
using System.Security.Cryptography;

namespace Nextag.Api.Services.Archivos.Implementation
{
    public class ArchivoAdjuntoService
        : IArchivoAdjuntoService
    {
        private readonly NextagContext context;

        private readonly IArchivoStorageService storage;

        private readonly ITicketAdjuntoTokenService
            tokenService;

        private readonly ILogger<ArchivoAdjuntoService>
            logger;

        private readonly long maximoBytesArchivo;

        // ==========================================
        // FORMATOS SOPORTADOS
        // ==========================================

        private static readonly Dictionary<string, string>
            FormatosPermitidos = new(
                StringComparer.OrdinalIgnoreCase)
            {
                [".jpg"] = "image/jpeg",
                [".png"] = "image/png",
                [".webp"] = "image/webp",
                [".pdf"] = "application/pdf"
            };

        public ArchivoAdjuntoService(
            NextagContext context,
            IArchivoStorageService storage,
            ITicketAdjuntoTokenService tokenService,
            IOptions<ArchivoStorageOptions> options,
            ILogger<ArchivoAdjuntoService> logger)
        {
            this.context = context;
            this.storage = storage;
            this.tokenService = tokenService;
            this.logger = logger;

            maximoBytesArchivo =
                options.Value.MaximoBytesArchivo;
        }

        // ==========================================
        // SUBIR ARCHIVO INTERNO
        // ==========================================

        public async Task<ArchivoAdjuntoResultadoDto>
            SubirInternoAsync(
                Guid empresaId,
                Guid ticketId,
                Guid usuarioId,
                bool esAdministrador,
                IFormFile archivo,
                CancellationToken ct = default)
        {
            if (empresaId == Guid.Empty ||
                ticketId == Guid.Empty ||
                usuarioId == Guid.Empty)
            {
                return ArchivoAdjuntoResultadoDto.Fail(
                    "Los identificadores proporcionados no son válidos.");
            }

            var ticket = await context.Tickets
                .AsNoTracking()
                .Where(x =>
                    x.Id == ticketId &&
                    x.EmpresaId == empresaId)
                .Select(x => new
                {
                    x.Id,
                    Estado = x.EstadoTicket.Clave
                })
                .FirstOrDefaultAsync(ct);

            if (ticket == null)
            {
                return ArchivoAdjuntoResultadoDto.Fail(
                    "Ticket no encontrado.");
            }

            if (ticket.Estado is "CERRADO" or "CANCELADO")
            {
                return ArchivoAdjuntoResultadoDto.Fail(
                    "No se pueden agregar archivos al ticket en su estado actual.");
            }

            // Confirmar que el usuario se encuentre
            // activo en la empresa.

            var usuarioValido = await context.Usuarios
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == usuarioId &&
                    x.Activo == true &&
                    x.UsuarioEmpresas.Any(ue =>
                        ue.EmpresaId == empresaId &&
                        ue.Activo == true),
                    ct);

            if (!usuarioValido)
            {
                return ArchivoAdjuntoResultadoDto.Fail(
                    "El usuario no tiene acceso a la empresa.");
            }

            // Administradores: pueden agregar evidencia.
            // Otros usuarios: deben ser integrantes
            // autorizados del equipo asignado.

            // ==========================================
            // VALIDAR PERMISOS DE SOPORTE
            // ==========================================

            if (!esAdministrador)
            {
                var tienePermiso = await context
                    .TicketAsignacions
                    .AsNoTracking()
                    .AnyAsync(x =>

                        // Empresa y ticket
                        x.EmpresaId == empresaId &&
                        x.TicketId == ticketId &&

                        // Asignación vigente
                        x.FechaFin == null &&

                        // Equipo activo
                        x.E.Activo == true &&

                        // Técnico autorizado
                        (
                            x.UsuarioTecnicoId == null ||
                            x.UsuarioTecnicoId == usuarioId
                        ) &&

                        // Integrante del equipo
                        x.E.EquipoSoporteIntegrantes.Any(i =>

                            i.EmpresaId == empresaId &&

                            i.UsuarioId == usuarioId &&

                            i.Activo == true &&

                            // Usuario activo
                            i.Usuario.Activo == true &&

                            // Usuario activo en la empresa
                            i.Usuario.UsuarioEmpresas.Any(ue =>
                                ue.EmpresaId == empresaId &&
                                ue.Activo == true)
                        ),
                        ct);

                if (!tienePermiso)
                {
                    return ArchivoAdjuntoResultadoDto.Fail(
                        "No tienes permiso para adjuntar archivos a este ticket.");
                }
            }

            return await GuardarArchivoAsync(
                empresaId,
                ticketId,
                usuarioId,
                archivo,
                null,
                ct);
        }

        // ==========================================
        // SUBIR ARCHIVO PUBLICO
        // ==========================================

        public async Task<ArchivoAdjuntoResultadoDto>
            SubirPublicoAsync(
                Guid ticketId,
                string token,
                IFormFile archivo,
                CancellationToken ct = default)
        {
            if (ticketId == Guid.Empty)
            {
                return ArchivoAdjuntoResultadoDto.Fail(
                    "La autorización no es válida.");
            }

            var autorizacion =
                await tokenService.ValidarAsync(
                    ticketId,
                    token,
                    ct);

            if (autorizacion == null)
            {
                return ArchivoAdjuntoResultadoDto.Fail(
                    "La autorización para adjuntar archivos no es válida.");
            }

            return await GuardarArchivoAsync(
                autorizacion.EmpresaId,
                ticketId,
                null,
                archivo,
                autorizacion.Id,
                ct);
        }

        // ==========================================
        // GUARDAR ARCHIVO
        // ==========================================

        private async Task<ArchivoAdjuntoResultadoDto>
            GuardarArchivoAsync(
                Guid empresaId,
                Guid ticketId,
                Guid? usuarioId,
                IFormFile archivo,
                Guid? autorizacionId,
                CancellationToken ct)
        {
            // ======================================
            // VALIDAR ARCHIVO
            // ======================================

            if (archivo == null || archivo.Length == 0)
            {
                return ArchivoAdjuntoResultadoDto.Fail(
                    "Debes seleccionar un archivo válido.");
            }

            if (archivo.Length > maximoBytesArchivo)
            {
                return ArchivoAdjuntoResultadoDto.Fail(
                    "El archivo supera el tamaño máximo permitido.");
            }

            var nombreOriginal = Path
                .GetFileName(archivo.FileName ?? string.Empty);

            if (string.IsNullOrWhiteSpace(nombreOriginal))
            {
                return ArchivoAdjuntoResultadoDto.Fail(
                    "El nombre del archivo no es válido.");
            }

            if (nombreOriginal.Length > 255)
            {
                return ArchivoAdjuntoResultadoDto.Fail(
                    "El nombre del archivo supera 255 caracteres.");
            }

            var extension = Path
                .GetExtension(nombreOriginal)
                .ToLowerInvariant();

            if (!FormatosPermitidos.TryGetValue(
                extension,
                out var mimeType))
            {
                return ArchivoAdjuntoResultadoDto.Fail(
                    "Formato no permitido. Utiliza JPG, PNG, WEBP o PDF.");
            }

            // ======================================
            // LEER Y VALIDAR CONTENIDO
            // ======================================

            byte[] contenido;

            await using (var origen = archivo.OpenReadStream())
            await using (var memoria = new MemoryStream())
            {
                var buffer = new byte[81920];
                long total = 0;

                int leidos;

                while ((leidos = await origen.ReadAsync(
                    buffer.AsMemory(),
                    ct)) > 0)
                {
                    total += leidos;

                    if (total > maximoBytesArchivo)
                    {
                        return ArchivoAdjuntoResultadoDto.Fail(
                            "El archivo supera el tamaño máximo permitido.");
                    }

                    await memoria.WriteAsync(
                        buffer.AsMemory(0, leidos),
                        ct);
                }

                contenido = memoria.ToArray();
            }

            if (contenido.Length == 0)
            {
                return ArchivoAdjuntoResultadoDto.Fail(
                    "El archivo está vacío.");
            }

            if (!FirmaValida(extension, contenido))
            {
                return ArchivoAdjuntoResultadoDto.Fail(
                    "El contenido no corresponde al formato del archivo.");
            }

            // ======================================
            // PREPARAR METADATOS
            // ======================================

            var archivoId = Guid.NewGuid();

            var ahora = DateTime.UtcNow;

            var hash = Convert.ToHexString(
                SHA256.HashData(contenido))
                .ToLowerInvariant();

            var rutaStorage =
                $"empresas/{empresaId:N}/" +
                $"tickets/{ticketId:N}/" +
                $"{archivoId:N}{extension}";

            var entidad = new ArchivoAdjunto
            {
                Id = archivoId,

                EmpresaId = empresaId,
                TicketId = ticketId,

                TicketSeguimientoId = null,
                OrdenTrabajoId = null,

                NombreOriginal = nombreOriginal,
                NombreAlmacenado =
                    $"{archivoId:N}{extension}",

                MimeType = mimeType,

                TamanoBytes = contenido.LongLength,

                RutaStorage = rutaStorage,

                HashSha256 = hash,

                VisibleSolicitante = true,

                EstadoAlmacenamiento = "PENDIENTE",

                Eliminado = false,

                FechaCreacion = ahora,

                UsuarioCreacion = usuarioId
            };

            // ======================================
            // RESERVAR CUPO Y CREAR PENDIENTE
            // ======================================

            await using (var transaction =
                await context.Database.BeginTransactionAsync(ct))
            {
                if (autorizacionId.HasValue)
                {
                    var reservado =
                        await tokenService.ReservarAsync(
                            autorizacionId.Value,
                            ct);

                    if (!reservado)
                    {
                        return ArchivoAdjuntoResultadoDto.Fail(
                            "La autorización expiró o se alcanzó el límite de archivos.");
                    }
                }

                context.ArchivoAdjuntos.Add(entidad);

                await context.SaveChangesAsync(ct);

                await transaction.CommitAsync(ct);

                // ==========================================
                // DESACOPLAR ENTIDAD DEL CONTEXTO
                // ==========================================

                // A partir de este punto las actualizaciones
                // de estado utilizarán ExecuteUpdateAsync.
                //
                // Evitamos mantener una entidad rastreada
                // con valores anteriores a los de SQL Server.

                context.Entry(entidad).State =
                    EntityState.Detached;
            }

            // ======================================
            // GUARDAR ARCHIVO FISICO
            // ======================================

            try
            {
                await using var stream =
                    new MemoryStream(
                        contenido,
                        writable: false);

                await storage.GuardarAsync(
                    stream,
                    empresaId,
                    ticketId,
                    archivoId,
                    extension,
                    ct);

                // ==================================
                // MARCAR DISPONIBLE
                // ==================================

                var filas = await context.ArchivoAdjuntos
                    .Where(x =>
                        x.Id == archivoId &&
                        x.EmpresaId == empresaId &&
                        x.EstadoAlmacenamiento == "PENDIENTE")
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(
                            x => x.EstadoAlmacenamiento,
                            "DISPONIBLE")
                        .SetProperty(
                            x => x.FechaActualizacion,
                            DateTime.UtcNow),
                        ct);

                if (filas != 1)
                {
                    throw new InvalidOperationException(
                        "No fue posible confirmar el almacenamiento.");
                }

                entidad.EstadoAlmacenamiento = "DISPONIBLE";

                entidad.FechaActualizacion = DateTime.UtcNow;

                return ArchivoAdjuntoResultadoDto.Ok(
                    Mapear(entidad));
            }
            catch (Exception ex)
            {
                // ==========================================
                // REGISTRAR ERROR
                // ==========================================

                logger.LogError(
                    ex,
                    "Error al almacenar adjunto {ArchivoId} " +
                    "del ticket {TicketId}. Empresa: {EmpresaId}",
                    archivoId,
                    ticketId,
                    empresaId);

                // ==========================================
                // RECUPERAR ESTADO Y RESERVA
                // ==========================================

                try
                {
                    // No utilizamos el CancellationToken de
                    // la solicitud porque el cliente pudo
                    // haberse desconectado.

                    await using var transaction =
                        await context.Database.BeginTransactionAsync(
                            CancellationToken.None);

                    // ======================================
                    // CAMBIAR PENDIENTE A ERROR
                    // ======================================

                    var fechaError = DateTime.UtcNow;

                    var filas = await context.ArchivoAdjuntos
                        .Where(x =>
                            x.Id == archivoId &&
                            x.EmpresaId == empresaId &&
                            x.EstadoAlmacenamiento == "PENDIENTE")
                        .ExecuteUpdateAsync(setters => setters

                            .SetProperty(
                                x => x.EstadoAlmacenamiento,
                                "ERROR")

                            .SetProperty(
                                x => x.FechaActualizacion,
                                fechaError),

                            CancellationToken.None);

                    // ======================================
                    // LIBERAR RESERVA PUBLICA
                    // ======================================

                    // Solamente liberamos el cupo cuando
                    // logramos cambiar el estado del archivo.
                    //
                    // Si el registro ya era ERROR o
                    // DISPONIBLE, no descontamos nuevamente.

                    if (
                        filas == 1 &&
                        autorizacionId.HasValue)
                    {
                        await tokenService.LiberarReservaAsync(
                            autorizacionId.Value,
                            CancellationToken.None);
                    }

                    // ======================================
                    // CONFIRMAR RECUPERACION
                    // ======================================

                    await transaction.CommitAsync(
                        CancellationToken.None);

                    logger.LogWarning(
                        "Adjunto {ArchivoId} marcado como ERROR. " +
                        "Reserva liberada: {ReservaLiberada}",
                        archivoId,
                        filas == 1 && autorizacionId.HasValue);
                }
                catch (Exception recuperacionEx)
                {
                    logger.LogError(
                        recuperacionEx,
                        "No fue posible recuperar el adjunto " +
                        "{ArchivoId} del ticket {TicketId}",
                        archivoId,
                        ticketId);
                }

                // ==========================================
                // RESPUESTA
                // ==========================================

                // Una desconexión del cliente puede provocar
                // OperationCanceledException.
                //
                // No ocultamos la cancelación devolviendo
                // un resultado de negocio aparentemente normal.

                if (ex is OperationCanceledException)
                {
                    throw;
                }

                return ArchivoAdjuntoResultadoDto.Fail(
                    "No fue posible almacenar el archivo.");
            }
        }

        // ==========================================
        // VALIDAR FIRMA REAL DEL ARCHIVO
        // ==========================================

        private static bool FirmaValida(
            string extension,
            byte[] contenido)
        {
            if (contenido.Length < 12)
            {
                return false;
            }

            return extension switch
            {
                ".jpg" =>
                    contenido[0] == 0xFF &&
                    contenido[1] == 0xD8 &&
                    contenido[2] == 0xFF,

                ".png" =>
                    contenido.AsSpan(0, 8).SequenceEqual(
                        new byte[]
                        {
                            0x89, 0x50, 0x4E, 0x47,
                            0x0D, 0x0A, 0x1A, 0x0A
                        }),

                ".webp" =>
                    contenido.AsSpan(0, 4).SequenceEqual(
                        "RIFF"u8) &&
                    contenido.AsSpan(8, 4).SequenceEqual(
                        "WEBP"u8),

                ".pdf" =>
                    contenido.AsSpan(0, 5).SequenceEqual(
                        "%PDF-"u8),

                _ => false
            };
        }

        // ==========================================
        // MAPEAR DTO
        // ==========================================

        private static ArchivoAdjuntoDto Mapear(
            ArchivoAdjunto entidad)
        {
            return new ArchivoAdjuntoDto
            {
                Id = entidad.Id,

                EmpresaId = entidad.EmpresaId,

                TicketId = entidad.TicketId,

                TicketSeguimientoId =
                    entidad.TicketSeguimientoId,

                NombreOriginal =
                    entidad.NombreOriginal,

                MimeType = entidad.MimeType,

                TamanoBytes = entidad.TamanoBytes,

                EstadoAlmacenamiento =
                    entidad.EstadoAlmacenamiento,

                VisibleSolicitante =
                    entidad.VisibleSolicitante,

                FechaCreacion =
                    entidad.FechaCreacion,

                UsuarioCreacion =
                    entidad.UsuarioCreacion
            };
        }
    }
}
