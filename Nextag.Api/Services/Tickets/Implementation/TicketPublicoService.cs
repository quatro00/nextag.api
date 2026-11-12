using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto.Tickets.Publicos;
using Nextag.Api.Services.Tickets.Interface;
using System.ComponentModel.DataAnnotations;

namespace Nextag.Api.Services.Tickets.Implementation
{
    public class TicketPublicoService : ITicketPublicoService
    {
        private readonly NextagContext context;

        private readonly ITicketFolioService folioService;

        public TicketPublicoService(
            NextagContext context,
            ITicketFolioService folioService)
        {
            this.context = context;
            this.folioService = folioService;
        }

        // ==========================================
        // VALIDAR ETIQUETA PUBLICA
        // ==========================================

        private async Task<(Guid EmpresaId, Guid ActivoId)?>
            ResolverEtiquetaAsync(
                string tokenPublico,
                CancellationToken ct)
        {
            tokenPublico =
                tokenPublico?.Trim().ToLowerInvariant()
                ?? string.Empty;

            if (
                tokenPublico.Length != 64 ||
                !tokenPublico.All(Uri.IsHexDigit)
            )
            {
                return null;
            }

            // Verificar etiqueta y empresa.

            var etiqueta = await context.Etiqueta
                .AsNoTracking()
                .Where(x =>
                    x.TokenPublico == tokenPublico &&
                    x.Anulada == false &&
                    x.PublicaHabilitada == true &&
                    x.Empresa.Activo == true)
                .Select(x => new
                {
                    x.Id,
                    x.EmpresaId
                })
                .FirstOrDefaultAsync(ct);

            if (etiqueta == null)
            {
                return null;
            }

            // Validar configuración pública.

            var configuracionValida =
                await context.EtiquetaConfiguracions
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.EmpresaId == etiqueta.EmpresaId &&
                        x.Activo == true &&
                        x.PermitirTicketPublico == true,
                        ct);

            if (!configuracionValida)
            {
                return null;
            }

            // Obtener la vinculación vigente.

            var vinculo = await context.ActivoEtiqueta
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == etiqueta.EmpresaId &&
                    x.EtiquetaId == etiqueta.Id &&
                    x.FechaDesvinculacion == null)
                .Select(x => new
                {
                    x.ActivoId
                })
                .SingleOrDefaultAsync(ct);

            if (vinculo == null)
            {
                return null;
            }

            // Validar que el activo pertenezca
            // a la misma empresa.

            var activoValido = await context.Activos
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == vinculo.ActivoId &&
                    x.EmpresaId == etiqueta.EmpresaId,
                    ct);

            if (!activoValido)
            {
                return null;
            }

            return (
                etiqueta.EmpresaId,
                vinculo.ActivoId
            );
        }

        // ==========================================
        // CATEGORIAS PUBLICAS
        // ==========================================

        public async Task<List<CategoriaTicketPublicaDto>?>
            GetCategoriasAsync(
                string tokenPublico,
                CancellationToken ct = default)
        {
            var contexto = await ResolverEtiquetaAsync(
                tokenPublico,
                ct);

            if (contexto == null)
            {
                return null;
            }

            return await context.CategoriaTickets
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == contexto.Value.EmpresaId &&
                    x.Activo == true)
                .OrderBy(x => x.Nombre)
                .Select(x => new CategoriaTicketPublicaDto
                {
                    Id = x.Id,
                    Nombre = x.Nombre
                })
                .ToListAsync(ct);
        }

        // ==========================================
        // CREAR TICKET PUBLICO
        // ==========================================

        public async Task<TicketPublicoResultadoDto?>
            CrearAsync(
                string tokenPublico,
                TicketPublicoCrearDto dto,
                CancellationToken ct = default)
        {
            // ======================================
            // VALIDAR TOKEN
            // ======================================

            var contexto = await ResolverEtiquetaAsync(
                tokenPublico,
                ct);

            if (contexto == null)
            {
                return null;
            }

            var empresaId = contexto.Value.EmpresaId;
            var activoId = contexto.Value.ActivoId;

            // ======================================
            // NORMALIZAR DATOS
            // ======================================

            var nombre = dto.Nombre?.Trim()
                ?? string.Empty;

            var correo = dto.Correo?.Trim();

            var telefono = dto.Telefono?.Trim();

            var titulo = dto.Titulo?.Trim()
                ?? string.Empty;

            var descripcion = dto.Descripcion?.Trim()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(correo))
            {
                correo = null;
            }

            if (string.IsNullOrWhiteSpace(telefono))
            {
                telefono = null;
            }

            // ======================================
            // VALIDACIONES
            // ======================================

            if (nombre.Length < 3 || nombre.Length > 150)
            {
                throw new ValidationException(
                    "El nombre debe contener entre 3 y 150 caracteres.");
            }

            if (correo == null && telefono == null)
            {
                throw new ValidationException(
                    "Debes proporcionar un correo o teléfono de contacto.");
            }

            if (correo != null &&
                (correo.Length > 254 ||
                 !new EmailAddressAttribute().IsValid(correo)))
            {
                throw new ValidationException(
                    "El correo electrónico no es válido.");
            }

            if (telefono != null && telefono.Length > 30)
            {
                throw new ValidationException(
                    "El teléfono no puede superar 30 caracteres.");
            }

            if (titulo.Length < 5 || titulo.Length > 200)
            {
                throw new ValidationException(
                    "El título debe contener entre 5 y 200 caracteres.");
            }

            if (descripcion.Length < 10 ||
                descripcion.Length > 5000)
            {
                throw new ValidationException(
                    "La descripción debe contener entre 10 y 5000 caracteres.");
            }

            // ======================================
            // CATEGORIA
            // ======================================

            if (dto.CategoriaTicketId.HasValue)
            {
                var categoriaValida =
                    await context.CategoriaTickets
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.Id == dto.CategoriaTicketId.Value &&
                            x.EmpresaId == empresaId &&
                            x.Activo == true,
                            ct);

                if (!categoriaValida)
                {
                    throw new ValidationException(
                        "La categoría seleccionada no es válida.");
                }
            }

            // ======================================
            // PRIORIDAD MEDIA
            // ======================================

            var prioridad = await context.PrioridadTickets
                .AsNoTracking()
                .Where(x =>
                    x.Clave == "MEDIA" &&
                    x.Activo == true)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(ct);

            if (prioridad == 0)
            {
                throw new InvalidOperationException(
                    "No está configurada la prioridad MEDIA.");
            }

            // ======================================
            // ESTADO ABIERTO
            // ======================================

            var estadoAbierto = await context.EstadoTickets
                .AsNoTracking()
                .Where(x =>
                    x.Clave == "ABIERTO" &&
                    x.Activo == true)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(ct);

            if (estadoAbierto == 0)
            {
                throw new InvalidOperationException(
                    "No está configurado el estado ABIERTO.");
            }

            // ======================================
            // TRANSACCION
            // ======================================

            await using var transaction =
                await context.Database.BeginTransactionAsync(ct);

            // Generación centralizada de folio.

            var numero = await folioService.SiguienteAsync(
                empresaId,
                ct);

            var folio = $"TIC-{numero:D6}";

            var ahora = DateTime.UtcNow;

            // ======================================
            // REGISTRO DEL TICKET
            // ======================================

            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),

                EmpresaId = empresaId,

                Numero = numero,
                Folio = folio,

                ActivoId = activoId,

                CategoriaTicketId =
                    dto.CategoriaTicketId,

                PrioridadTicketId = prioridad,

                EstadoTicketId = estadoAbierto,

                Origen = "PUBLICO",

                Titulo = titulo,
                Descripcion = descripcion,

                SolicitanteUsuarioId = null,

                SolicitanteNombre = nombre,
                SolicitanteCorreo = correo,
                SolicitanteTelefono = telefono,

                FechaApertura = ahora,

                FechaCreacion = ahora,

                UsuarioCreacion = null
            };

            context.Tickets.Add(ticket);

            // ======================================
            // HISTORIAL INICIAL
            // ======================================

            var historial = new TicketEstadoHistorial
            {
                Id = Guid.NewGuid(),

                EmpresaId = empresaId,
                TicketId = ticket.Id,

                EstadoAnteriorId = null,
                EstadoNuevoId = estadoAbierto,

                Motivo = "Ticket creado desde etiqueta NFC/QR.",

                FechaCambio = ahora,

                UsuarioId = null
            };

            context.TicketEstadoHistorials.Add(historial);

            // ======================================
            // GUARDAR
            // ======================================

            await context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);

            return new TicketPublicoResultadoDto
            {
                Id = ticket.Id,

                Folio = ticket.Folio,

                FechaApertura = ticket.FechaApertura,

                Mensaje = "Ticket registrado correctamente."
            };
        }
    }
}
