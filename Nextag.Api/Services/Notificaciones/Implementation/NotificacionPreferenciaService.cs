using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto.Notificaciones;
using Nextag.Api.Services.Notificaciones.Interface;

namespace Nextag.Api.Services.Notificaciones.Implementation
{
    public class NotificacionPreferenciaService
        : INotificacionPreferenciaService
    {
        private readonly NextagContext context;
        private const string VersionAvisoWhatsapp = "v1";

        public NotificacionPreferenciaService(
            NextagContext context)
        {
            this.context = context;
        }

        // ==========================================
        // VALIDAR USUARIO Y EMPRESA
        // ==========================================

        private async Task<Usuario> ValidarUsuarioAsync(
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct)
        {
            var usuario = await context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == usuarioId &&
                    x.Activo == true,
                    ct);

            if (usuario == null)
            {
                throw new UnauthorizedAccessException(
                    "El usuario no está activo.");
            }

            var pertenece = await context.UsuarioEmpresas
                .AsNoTracking()
                .AnyAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.UsuarioId == usuarioId &&
                    x.Activo == true,
                    ct);

            if (!pertenece)
            {
                throw new UnauthorizedAccessException(
                    "El usuario no pertenece a la empresa.");
            }

            return usuario;
        }

        // ==========================================
        // OBTENER PREFERENCIAS
        // ==========================================

        public async Task<List<NotificacionPreferenciaDto>>
            ObtenerAsync(
                Guid empresaId,
                Guid usuarioId,
                CancellationToken ct = default)
        {
            var usuario = await ValidarUsuarioAsync(
                empresaId,
                usuarioId,
                ct);

            // ======================================
            // EVENTOS ACTIVOS
            // ======================================

            var eventos = await context
                .NotificacionEventos
                .AsNoTracking()
                .Where(x => x.Activo == true)
                .OrderBy(x => x.Id)
                .ToListAsync(ct);

            // ======================================
            // PREFERENCIAS DEL USUARIO
            // ======================================

            var preferencias = await context
                .UsuarioNotificacionPreferencia
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.UsuarioId == usuarioId)
                .ToListAsync(ct);

            var correoDisponible =
                !string.IsNullOrWhiteSpace(usuario.Email);

            var whatsappDisponible =
                !string.IsNullOrWhiteSpace(
                    usuario.Celular ?? usuario.Telefono);

            // ======================================
            // GENERAR RESULTADO
            // ======================================

            var resultado =
                new List<NotificacionPreferenciaDto>();

            foreach (var evento in eventos)
            {
                var configuraciones = preferencias
                    .Where(x =>
                        x.NotificacionEventoId == evento.Id)
                    .ToList();

                var interna = configuraciones
                    .FirstOrDefault(x =>
                        x.Canal == "INTERNA");

                var correo = configuraciones
                    .FirstOrDefault(x =>
                        x.Canal == "CORREO");

                var whatsapp = configuraciones
                    .FirstOrDefault(x =>
                        x.Canal == "WHATSAPP");

                resultado.Add(
                    new NotificacionPreferenciaDto
                    {
                        EventoId = evento.Id,

                        EventoClave = evento.Clave,

                        EventoNombre = evento.Nombre,

                        EventoDescripcion =
                            evento.Descripcion,

                        Interna =
                            interna?.Habilitado ?? true,

                        Correo =
                            (correo?.Habilitado ?? true) &&
                            correoDisponible,

                        Whatsapp =
                            (whatsapp?.Habilitado ?? false) &&
                            whatsapp?.ConsentimientoWhatsapp == true &&
                            whatsapp.FechaConsentimiento.HasValue &&
                            whatsappDisponible,

                        ConsentimientoWhatsapp =
                            whatsapp?.ConsentimientoWhatsapp
                                ?? false,

                        FechaConsentimientoWhatsapp =
                            whatsapp?.FechaConsentimiento,

                        VersionAviso =
                            whatsapp?.VersionAviso,

                        CorreoDisponible =
                            correoDisponible,

                        WhatsappDisponible =
                            whatsappDisponible
                    });
            }

            return resultado;
        }

        // ==========================================
        // GUARDAR PREFERENCIAS
        // ==========================================

        public async Task<NotificacionPreferenciaDto>
            GuardarAsync(
                Guid empresaId,
                Guid usuarioId,
                int eventoId,
                NotificacionPreferenciaGuardarDto dto,
                CancellationToken ct = default)
        {
            var usuario = await ValidarUsuarioAsync(
                empresaId,
                usuarioId,
                ct);

            // ======================================
            // VALIDAR EVENTO
            // ======================================

            var evento = await context
                .NotificacionEventos
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == eventoId &&
                    x.Activo == true,
                    ct);

            if (evento == null)
            {
                throw new ArgumentException(
                    "El evento no existe o no está activo.");
            }

            // ======================================
            // VALIDAR CORREO
            // ======================================

            var correoDisponible =
                !string.IsNullOrWhiteSpace(usuario.Email);

            if (dto.Correo && !correoDisponible)
            {
                throw new ArgumentException(
                    "El usuario no tiene un correo registrado.");
            }

            // ======================================
            // VALIDAR WHATSAPP
            // ======================================

            var whatsappDisponible =
                !string.IsNullOrWhiteSpace(
                    usuario.Celular ?? usuario.Telefono);

            if (dto.Whatsapp)
            {
                if (!whatsappDisponible)
                {
                    throw new ArgumentException(
                        "El usuario no tiene teléfono registrado.");
                }

                if (!dto.ConsentimientoWhatsapp)
                {
                    throw new ArgumentException(
                        "Es necesario aceptar el consentimiento de WhatsApp.");
                }

                if (dto.VersionAviso != VersionAvisoWhatsapp)
                {
                    throw new ArgumentException(
                        "La versión del aviso de WhatsApp no está vigente.");
                }
            }

            // ======================================
            // PREFERENCIAS EXISTENTES
            // ======================================

            var preferencias = await context
                .UsuarioNotificacionPreferencia
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.UsuarioId == usuarioId &&
                    x.NotificacionEventoId == eventoId)
                .ToListAsync(ct);

            var ahora = DateTime.UtcNow;

            // ======================================
            // INTERNA
            // ======================================

            GuardarCanal(
                preferencias,
                empresaId,
                usuarioId,
                eventoId,
                "INTERNA",
                dto.Interna,
                false,
                null,
                null,
                ahora);

            // ======================================
            // CORREO
            // ======================================

            GuardarCanal(
                preferencias,
                empresaId,
                usuarioId,
                eventoId,
                "CORREO",
                dto.Correo,
                false,
                null,
                null,
                ahora);

            // ======================================
            // WHATSAPP
            // ======================================

            var preferenciaWhatsapp = preferencias
                .FirstOrDefault(x =>
                    x.Canal == "WHATSAPP");

            DateTime? fechaConsentimiento = null;

            string? versionAviso = null;

            if (dto.Whatsapp && dto.ConsentimientoWhatsapp)
            {
                // Conservar fecha solamente cuando
                // el consentimiento anterior sigue
                // vigente para la misma versión.

                if (
                    preferenciaWhatsapp?.ConsentimientoWhatsapp == true &&
                    preferenciaWhatsapp.FechaConsentimiento.HasValue &&
                    preferenciaWhatsapp.VersionAviso ==
                        dto.VersionAviso
                )
                {
                    fechaConsentimiento =
                        preferenciaWhatsapp.FechaConsentimiento;
                }
                else
                {
                    fechaConsentimiento = ahora;
                }

                versionAviso = dto.VersionAviso;
            }

            GuardarCanal(
                preferencias,
                empresaId,
                usuarioId,
                eventoId,
                "WHATSAPP",
                dto.Whatsapp,
                dto.Whatsapp && dto.ConsentimientoWhatsapp,
                fechaConsentimiento,
                versionAviso,
                ahora);

            // ======================================
            // GUARDAR CAMBIOS
            // ======================================

            await context.SaveChangesAsync(ct);

            // ======================================
            // DEVOLVER CONFIGURACION
            // ======================================

            return new NotificacionPreferenciaDto
            {
                EventoId = evento.Id,

                EventoClave = evento.Clave,

                EventoNombre = evento.Nombre,

                EventoDescripcion =
                    evento.Descripcion,

                Interna = dto.Interna,

                Correo = dto.Correo,

                Whatsapp = dto.Whatsapp,

                ConsentimientoWhatsapp =
                    dto.Whatsapp && dto.ConsentimientoWhatsapp,

                FechaConsentimientoWhatsapp =
                    fechaConsentimiento,

                VersionAviso = versionAviso,

                CorreoDisponible =
                    correoDisponible,

                WhatsappDisponible =
                    whatsappDisponible
            };
        }

        // ==========================================
        // INSERTAR O ACTUALIZAR CANAL
        // ==========================================

        private void GuardarCanal(
            List<UsuarioNotificacionPreferencium> preferencias,
            Guid empresaId,
            Guid usuarioId,
            int eventoId,
            string canal,
            bool habilitado,
            bool consentimientoWhatsapp,
            DateTime? fechaConsentimiento,
            string? versionAviso,
            DateTime ahora)
        {
            var preferencia = preferencias
                .FirstOrDefault(x =>
                    x.Canal == canal);

            // ======================================
            // INSERTAR
            // ======================================

            if (preferencia == null)
            {
                preferencia =
                    new UsuarioNotificacionPreferencium
                    {
                        Id = Guid.NewGuid(),

                        EmpresaId = empresaId,

                        UsuarioId = usuarioId,

                        NotificacionEventoId = eventoId,

                        Canal = canal,

                        Habilitado = habilitado,

                        ConsentimientoWhatsapp =
                            consentimientoWhatsapp,

                        FechaConsentimiento =
                            fechaConsentimiento,

                        VersionAviso =
                            versionAviso,

                        FechaCreacion = ahora,

                        UsuarioCreacion = usuarioId
                    };

                context.UsuarioNotificacionPreferencia
                    .Add(preferencia);

                preferencias.Add(preferencia);
            }
            else
            {
                // ==================================
                // ACTUALIZAR
                // ==================================

                preferencia.Habilitado = habilitado;

                preferencia.ConsentimientoWhatsapp =
                    consentimientoWhatsapp;

                preferencia.FechaConsentimiento =
                    fechaConsentimiento;

                preferencia.VersionAviso =
                    versionAviso;

                preferencia.FechaActualizacion = ahora;

                preferencia.UsuarioModificacion =
                    usuarioId;
            }
        }
    }
}
