using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Dto.Notificaciones;
using Nextag.Api.Services.Notificaciones.Interface;
using System.Net;
using System.Text;

namespace Nextag.Api.Services.Notificaciones.Implementation
{
    public class NotificacionCorreoService
        : INotificacionCorreoService
    {
        private readonly NextagContext context;

        private readonly INotificacionSmtpService smtpService;

        private readonly ILogger<NotificacionCorreoService> logger;

        private const int MinutosBloqueo = 15;

        public NotificacionCorreoService(
            NextagContext context,
            INotificacionSmtpService smtpService,
            ILogger<NotificacionCorreoService> logger)
        {
            this.context = context;
            this.smtpService = smtpService;
            this.logger = logger;
        }

        // ==========================================
        // PROCESAR CORREOS PENDIENTES
        // ==========================================

        public async Task<NotificacionCorreoResultadoDto>
            ProcesarPendientesAsync(
                int cantidad = 20,
                CancellationToken ct = default)
        {
            cantidad = Math.Clamp(cantidad, 1, 100);

            var resultado = new NotificacionCorreoResultadoDto
            {
                FechaInicio = DateTime.UtcNow
            };

            // ======================================
            // RECUPERAR ENVIOS AGOTADOS
            // ======================================

            await RecuperarAgotadosAsync(ct);

            var ahora = DateTime.UtcNow;

            var limiteBloqueo =
                ahora.AddMinutes(-MinutosBloqueo);

            // ======================================
            // CONSULTAR CANDIDATOS
            // ======================================

            var candidatos = await context
                .NotificacionEnvios
                .AsNoTracking()
                .Where(x =>
                    x.Canal == "CORREO" &&
                    x.Intentos < x.MaxIntentos &&
                    (
                        (
                            (x.Estado == "PENDIENTE" ||
                             x.Estado == "ERROR") &&
                            (
                                x.FechaProximoIntento == null ||
                                x.FechaProximoIntento <= ahora
                            )
                        )
                        ||
                        (
                            x.Estado == "PROCESANDO" &&
                            x.FechaBloqueo < limiteBloqueo
                        )
                    ))
                .OrderBy(x => x.FechaCreacion)
                .Select(x => x.Id)
                .Take(cantidad)
                .ToListAsync(ct);

            resultado.Encontrados = candidatos.Count;

            // ======================================
            // PROCESAR CADA ENVIO
            // ======================================

            foreach (var envioId in candidatos)
            {
                ct.ThrowIfCancellationRequested();

                var token = Guid.NewGuid();

                var adquirido = await AdquirirAsync(
                    envioId,
                    token,
                    ct);

                if (!adquirido)
                {
                    resultado.Omitidos++;
                    continue;
                }

                try
                {
                    await ProcesarEnvioAsync(
                        envioId,
                        token,
                        ct);

                    resultado.Enviados++;
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
                        "Error procesando el envío {EnvioId}",
                        envioId);

                    context.ChangeTracker.Clear();

                    try
                    {
                        await RegistrarErrorAsync(
                            envioId,
                            token,
                            ex,
                            ct);
                    }
                    catch (Exception registroEx)
                    {
                        logger.LogError(
                            registroEx,
                            "No fue posible registrar el error del envío {EnvioId}",
                            envioId);
                    }
                }
            }

            resultado.FechaFin = DateTime.UtcNow;

            return resultado;
        }

        // ==========================================
        // RECUPERAR ENVIOS AGOTADOS
        // ==========================================

        private async Task<int> RecuperarAgotadosAsync(
            CancellationToken ct)
        {
            var ahora = DateTime.UtcNow;

            var limiteBloqueo =
                ahora.AddMinutes(-MinutosBloqueo);

            return await context.NotificacionEnvios
                .Where(x =>
                    x.Canal == "CORREO" &&
                    x.Estado == "PROCESANDO" &&
                    x.Intentos >= x.MaxIntentos &&
                    x.FechaBloqueo < limiteBloqueo)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            x => x.Estado,
                            "ERROR")
                        .SetProperty(
                            x => x.FechaBloqueo,
                            (DateTime?)null)
                        .SetProperty(
                            x => x.BloqueoToken,
                            (Guid?)null)
                        .SetProperty(
                            x => x.FechaProximoIntento,
                            (DateTime?)null)
                        .SetProperty(
                            x => x.UltimoError,
                            "El envío fue interrumpido y agotó sus intentos.")
                        .SetProperty(
                            x => x.FechaActualizacion,
                            ahora),
                    ct);
        }

        // ==========================================
        // ADQUIRIR ENVIO
        // ==========================================

        private async Task<bool> AdquirirAsync(
            Guid envioId,
            Guid token,
            CancellationToken ct)
        {
            var ahora = DateTime.UtcNow;

            var limiteBloqueo =
                ahora.AddMinutes(-MinutosBloqueo);

            var actualizados = await context
                .NotificacionEnvios
                .Where(x =>
                    x.Id == envioId &&
                    x.Canal == "CORREO" &&
                    x.Intentos < x.MaxIntentos &&
                    (
                        (
                            (
                                x.Estado == "PENDIENTE" ||
                                x.Estado == "ERROR"
                            ) &&
                            (
                                x.FechaProximoIntento == null ||
                                x.FechaProximoIntento <= ahora
                            )
                        )
                        ||
                        (
                            x.Estado == "PROCESANDO" &&
                            x.FechaBloqueo < limiteBloqueo
                        )
                    ))
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            x => x.Estado,
                            "PROCESANDO")
                        .SetProperty(
                            x => x.BloqueoToken,
                            token)
                        .SetProperty(
                            x => x.FechaBloqueo,
                            ahora)
                        .SetProperty(
                            x => x.Intentos,
                            x => x.Intentos + 1)
                        .SetProperty(
                            x => x.FechaUltimoIntento,
                            ahora)
                        .SetProperty(
                            x => x.FechaActualizacion,
                            ahora),
                    ct);

            return actualizados == 1;
        }

        // ==========================================
        // PROCESAR ENVIO SMTP
        // ==========================================

        private async Task ProcesarEnvioAsync(
            Guid envioId,
            Guid token,
            CancellationToken ct)
        {
            // ======================================
            // CONSULTAR ENVIO
            // ======================================

            var envio = await context.NotificacionEnvios
                .AsNoTracking()
                .Where(x =>
                    x.Id == envioId &&
                    x.Canal == "CORREO" &&
                    x.Estado == "PROCESANDO" &&
                    x.BloqueoToken == token)
                .Select(x => new
                {
                    x.Id,
                    x.EmpresaId,
                    x.Destino,

                    NotificacionEmpresaId =
                        x.Notificacion.EmpresaId,

                    Titulo =
                        x.Notificacion.Titulo,

                    Mensaje =
                        x.Notificacion.Mensaje,

                    TicketId =
                        x.Notificacion.TicketId
                })
                .FirstOrDefaultAsync(ct);

            if (envio == null)
            {
                throw new InvalidOperationException(
                    "El envío ya no pertenece a esta ejecución.");
            }

            if (envio.EmpresaId != envio.NotificacionEmpresaId)
            {
                throw new InvalidOperationException(
                    "La empresa del envío no coincide con su notificación.");
            }

            // ======================================
            // VALIDAR BLOQUEO ANTES DE ENVIAR
            // ======================================

            var ahora = DateTime.UtcNow;

            var bloqueoVigente = await context.NotificacionEnvios
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == envioId &&
                    x.Estado == "PROCESANDO" &&
                    x.BloqueoToken == token &&
                    x.FechaBloqueo >
                        ahora.AddMinutes(-MinutosBloqueo),
                    ct);

            if (!bloqueoVigente)
            {
                throw new InvalidOperationException(
                    "El bloqueo del envío expiró.");
            }

            // ======================================
            // CONSTRUIR MENSAJE
            // ======================================

            var asunto = $"Nextag | {envio.Titulo}";

            var cuerpoHtml = ConstruirHtml(
                envio.Titulo,
                envio.Mensaje);

            // ======================================
            // ENVIAR POR SMTP
            // ======================================

            var messageId = await smtpService.EnviarAsync(
                envio.Destino,
                asunto,
                cuerpoHtml,
                ct);

            // ======================================
            // MARCAR COMO ENVIADO
            // ======================================

            var fechaEnvio = DateTime.UtcNow;

            var actualizados = await context.NotificacionEnvios
                .Where(x =>
                    x.Id == envioId &&
                    x.Estado == "PROCESANDO" &&
                    x.BloqueoToken == token)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            x => x.Estado,
                            "ENVIADO")
                        .SetProperty(
                            x => x.FechaEnviado,
                            fechaEnvio)
                        .SetProperty(
                            x => x.Proveedor,
                            "ZOHO_SMTP")
                        .SetProperty(
                            x => x.IdentificadorProveedor,
                            messageId)
                        .SetProperty(
                            x => x.UltimoError,
                            (string?)null)
                        .SetProperty(
                            x => x.FechaProximoIntento,
                            (DateTime?)null)
                        .SetProperty(
                            x => x.FechaBloqueo,
                            (DateTime?)null)
                        .SetProperty(
                            x => x.BloqueoToken,
                            (Guid?)null)
                        .SetProperty(
                            x => x.FechaActualizacion,
                            fechaEnvio),
                    ct);

            if (actualizados != 1)
            {
                throw new InvalidOperationException(
                    "El servidor SMTP aceptó el mensaje, pero no fue posible confirmar el estado del envío.");
            }

            logger.LogInformation(
                "Envío {EnvioId} aceptado por SMTP.",
                envioId);
        }

        // ==========================================
        // CONSTRUIR HTML DEL CORREO
        // ==========================================

        private static string ConstruirHtml(
            string titulo,
            string mensaje)
        {
            var tituloSeguro = WebUtility.HtmlEncode(
                titulo);

            var mensajeSeguro = WebUtility.HtmlEncode(
                mensaje)
                .Replace("\r\n", "<br>")
                .Replace("\n", "<br>");

            var html = new StringBuilder();

            html.Append("""
                <!DOCTYPE html>
                <html lang="es">
                <head>
                    <meta charset="UTF-8">
                    <meta name="viewport"
                          content="width=device-width, initial-scale=1">
                </head>

                <body style="
                    margin:0;
                    padding:0;
                    background-color:#f4f6f8;
                    font-family:Arial,Helvetica,sans-serif;">

                    <div style="
                        max-width:600px;
                        margin:30px auto;
                        background:#ffffff;
                        border-radius:10px;
                        overflow:hidden;
                        border:1px solid #e5e7eb;">

                        <div style="
                            background:#1565c0;
                            color:#ffffff;
                            padding:25px 30px;">

                            <h1 style="
                                margin:0;
                                font-size:24px;
                                font-weight:600;">

                                NEXTAG

                            </h1>

                            <p style="
                                margin:8px 0 0;
                                font-size:13px;">

                                Inventario y soporte técnico

                            </p>

                        </div>

                        <div style="padding:30px;">

                """);

            html.Append($"""
                        <h2 style="
                            color:#1f2937;
                            font-size:20px;
                            margin-top:0;">

                            {tituloSeguro}

                        </h2>

                        <p style="
                            color:#4b5563;
                            font-size:15px;
                            line-height:1.6;">

                            {mensajeSeguro}

                        </p>
                """);

            html.Append("""
                        </div>

                        <div style="
                            background:#f9fafb;
                            padding:20px 30px;
                            border-top:1px solid #e5e7eb;
                            text-align:center;">

                            <p style="
                                margin:0;
                                color:#6b7280;
                                font-size:12px;">

                                Este es un mensaje automático
                                generado por Nextag.

                            </p>

                            <p style="
                                margin:8px 0 0;
                                color:#9ca3af;
                                font-size:12px;">

                                Por favor, no respondas a este correo.

                            </p>

                        </div>

                    </div>

                </body>
                </html>
                """);

            return html.ToString();
        }

        // ==========================================
        // REGISTRAR ERROR
        // ==========================================

        private async Task RegistrarErrorAsync(
            Guid envioId,
            Guid token,
            Exception ex,
            CancellationToken ct)
        {
            var envio = await context.NotificacionEnvios
                .AsNoTracking()
                .Where(x =>
                    x.Id == envioId &&
                    x.Canal == "CORREO" &&
                    x.Estado == "PROCESANDO" &&
                    x.BloqueoToken == token)
                .Select(x => new
                {
                    x.Intentos,
                    x.MaxIntentos
                })
                .FirstOrDefaultAsync(ct);

            if (envio == null)
            {
                return;
            }

            var ahora = DateTime.UtcNow;

            var agotado =
                envio.Intentos >= envio.MaxIntentos;

            // ======================================
            // BACKOFF EXPONENCIAL
            // ======================================

            var minutos = Math.Min(
                60,
                (int)Math.Pow(
                    2,
                    Math.Min(envio.Intentos, 6)));

            var proximoIntento = agotado
                ? (DateTime?)null
                : ahora.AddMinutes(minutos);

            var error = ex.Message;

            if (error.Length > 1000)
            {
                error = error[..1000];
            }

            // ======================================
            // ACTUALIZAR ESTADO
            // ======================================

            await context.NotificacionEnvios
                .Where(x =>
                    x.Id == envioId &&
                    x.Canal == "CORREO" &&
                    x.Estado == "PROCESANDO" &&
                    x.BloqueoToken == token)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            x => x.Estado,
                            "ERROR")
                        .SetProperty(
                            x => x.UltimoError,
                            error)
                        .SetProperty(
                            x => x.FechaProximoIntento,
                            proximoIntento)
                        .SetProperty(
                            x => x.FechaBloqueo,
                            (DateTime?)null)
                        .SetProperty(
                            x => x.BloqueoToken,
                            (Guid?)null)
                        .SetProperty(
                            x => x.FechaActualizacion,
                            ahora),
                    ct);
        }
    }
}