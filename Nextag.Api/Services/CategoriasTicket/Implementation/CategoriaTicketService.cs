using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto.CategoriasTicket;
using Nextag.Api.Models.Dto;
using Nextag.Api.Services.CategoriasTicket.Interface;

namespace Nextag.Api.Services.CategoriasTicket.Implementation
{
    public class CategoriaTicketService : ICategoriaTicketService
    {
        private readonly NextagContext context;

        public CategoriaTicketService(NextagContext context)
        {
            this.context = context;
        }

        // ==========================================
        // LISTADO
        // ==========================================

        public async Task<List<CategoriaTicketListDto>> GetAllAsync(
            Guid empresaId,
            bool? activo = null,
            string? buscar = null,
            CancellationToken ct = default)
        {
            var query = context.CategoriaTickets
                .AsNoTracking()
                .Where(x => x.EmpresaId == empresaId);

            if (activo.HasValue)
            {
                query = query.Where(x =>
                    x.Activo == activo.Value);
            }

            buscar = buscar?.Trim();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                query = query.Where(x =>
                    x.Clave.Contains(buscar) ||
                    x.Nombre.Contains(buscar) ||
                    (x.Descripcion != null &&
                     x.Descripcion.Contains(buscar)));
            }

            return await query
                .OrderBy(x => x.Nombre)
                .Select(x => new CategoriaTicketListDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,
                    Clave = x.Clave,
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion,
                    Activo = x.Activo == true
                })
                .ToListAsync(ct);
        }

        // ==========================================
        // DETALLE
        // ==========================================

        public async Task<CategoriaTicketDetalleDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default)
        {
            return await context.CategoriaTickets
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId)
                .Select(x => new CategoriaTicketDetalleDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,

                    Clave = x.Clave,
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion,

                    Activo = x.Activo == true,

                    FechaCreacion = x.FechaCreacion,
                    UsuarioCreacion = x.UsuarioCreacion,

                    FechaActualizacion = x.FechaActualizacion,
                    UsuarioModificacion = x.UsuarioModificacion
                })
                .FirstOrDefaultAsync(ct);
        }

        // ==========================================
        // CREAR
        // ==========================================

        public async Task<ServiceResult<CategoriaTicketDetalleDto>> CreateAsync(
            CategoriaTicketGuardarDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default)
        {
            var error = await ValidarDatosAsync(
                dto,
                empresaId,
                null,
                ct);

            if (error != null)
            {
                return ServiceResult<CategoriaTicketDetalleDto>
                    .Fail(error);
            }

            var categoria = new CategoriaTicket
            {
                EmpresaId = empresaId,

                Clave = dto.Clave.Trim().ToUpperInvariant(),
                Nombre = dto.Nombre.Trim(),
                Descripcion = Normalizar(dto.Descripcion),

                Activo = true,

                FechaCreacion = DateTime.UtcNow,
                UsuarioCreacion = usuarioId
            };

            context.CategoriaTickets.Add(categoria);

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (EsDuplicado(ex))
            {
                return ServiceResult<CategoriaTicketDetalleDto>
                    .Fail(
                        "Ya existe una categoría de ticket con esa clave en la empresa."
                    );
            }

            var detalle = await GetByIdAsync(
                categoria.Id,
                empresaId,
                ct);

            return ServiceResult<CategoriaTicketDetalleDto>
                .Ok(detalle!);
        }

        // ==========================================
        // ACTUALIZAR
        // ==========================================

        public async Task<ServiceResult<CategoriaTicketDetalleDto>> UpdateAsync(
            Guid id,
            CategoriaTicketGuardarDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default)
        {
            var categoria = await context.CategoriaTickets
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId,
                    ct);

            if (categoria == null)
            {
                return ServiceResult<CategoriaTicketDetalleDto>
                    .Fail("Categoría de ticket no encontrada.");
            }

            var error = await ValidarDatosAsync(
                dto,
                empresaId,
                id,
                ct);

            if (error != null)
            {
                return ServiceResult<CategoriaTicketDetalleDto>
                    .Fail(error);
            }

            categoria.Clave = dto.Clave.Trim().ToUpperInvariant();
            categoria.Nombre = dto.Nombre.Trim();
            categoria.Descripcion = Normalizar(dto.Descripcion);

            categoria.FechaActualizacion = DateTime.UtcNow;
            categoria.UsuarioModificacion = usuarioId;

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (EsDuplicado(ex))
            {
                return ServiceResult<CategoriaTicketDetalleDto>
                    .Fail(
                        "Ya existe otra categoría de ticket con esa clave en la empresa."
                    );
            }

            var detalle = await GetByIdAsync(
                id,
                empresaId,
                ct);

            return ServiceResult<CategoriaTicketDetalleDto>
                .Ok(detalle!);
        }

        // ==========================================
        // ACTIVAR / DESACTIVAR
        // ==========================================

        public async Task<ServiceResult<bool>> UpdateStatusAsync(
            Guid id,
            Guid empresaId,
            bool activo,
            Guid usuarioId,
            CancellationToken ct = default)
        {
            var categoria = await context.CategoriaTickets
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId,
                    ct);

            if (categoria == null)
            {
                return ServiceResult<bool>
                    .Fail("Categoría de ticket no encontrada.");
            }

            if (categoria.Activo == activo)
            {
                return ServiceResult<bool>.Ok(true);
            }

            categoria.Activo = activo;

            categoria.FechaActualizacion = DateTime.UtcNow;
            categoria.UsuarioModificacion = usuarioId;

            await context.SaveChangesAsync(ct);

            return ServiceResult<bool>.Ok(true);
        }

        // ==========================================
        // VALIDACIONES
        // ==========================================

        private async Task<string?> ValidarDatosAsync(
            CategoriaTicketGuardarDto dto,
            Guid empresaId,
            Guid? excluirId,
            CancellationToken ct)
        {
            var clave = dto.Clave?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(clave))
            {
                return "La clave de la categoría es obligatoria.";
            }

            if (clave.Length > 40)
            {
                return "La clave no puede superar 40 caracteres.";
            }

            var nombre = dto.Nombre?.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                return "El nombre de la categoría es obligatorio.";
            }

            if (nombre.Length > 150)
            {
                return "El nombre no puede superar 150 caracteres.";
            }

            var descripcion = Normalizar(dto.Descripcion);

            if (descripcion?.Length > 500)
            {
                return "La descripción no puede superar 500 caracteres.";
            }

            var existe = await context.CategoriaTickets
                .AsNoTracking()
                .AnyAsync(x =>
                    x.EmpresaId == empresaId &&
                    x.Clave == clave &&
                    (!excluirId.HasValue || x.Id != excluirId.Value),
                    ct);

            if (existe)
            {
                return "Ya existe una categoría de ticket con esa clave en la empresa.";
            }

            return null;
        }

        // ==========================================
        // HELPERS
        // ==========================================

        private static string? Normalizar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? null
                : valor.Trim();
        }

        private static bool EsDuplicado(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sql &&
                   (sql.Number == 2601 || sql.Number == 2627);
        }
    }
}
