using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto.TiposContrato;
using Nextag.Api.Models.Dto;
using Nextag.Api.Services.TiposContrato.Interface;

namespace Nextag.Api.Services.TiposContrato.Implementation
{
    public class TipoContratoService : ITipoContratoService
    {
        private readonly NextagContext context;

        public TipoContratoService(NextagContext context)
        {
            this.context = context;
        }

        public Task<List<TipoContratoListDto>> GetAllAsync(
            Guid empresaId,
            bool? activo = null,
            string? buscar = null,
            CancellationToken ct = default)
        {
            var query = context.TipoContratos
                .AsNoTracking()
                .Where(x => x.EmpresaId == empresaId);

            if (activo.HasValue)
            {
                if (activo.Value)
                    query = query.Where(x => x.Activo == true);
                else
                    query = query.Where(x => x.Activo != true);
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

            return query
                .OrderBy(x => x.Nombre)
                .ThenBy(x => x.Clave)
                .Select(x => new TipoContratoListDto
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

        public Task<TipoContratoDetalleDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default)
        {
            return context.TipoContratos
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId)
                .Select(x => new TipoContratoDetalleDto
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

        public async Task<ServiceResult<TipoContratoDetalleDto>> CreateAsync(
            TipoContratoGuardarDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default)
        {
            var error = await ValidarDatosAsync(
                dto,
                empresaId,
                null,
                ct);

            if (error is not null)
            {
                return ServiceResult<TipoContratoDetalleDto>
                    .Fail(error);
            }

            var tipoContrato = new TipoContrato
            {
                EmpresaId = empresaId,
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                UsuarioCreacion = usuarioId
            };

            MapearCampos(tipoContrato, dto);

            try
            {
                context.TipoContratos.Add(tipoContrato);

                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (EsDuplicado(ex))
            {
                return ServiceResult<TipoContratoDetalleDto>.Fail(
                    "Ya existe un tipo de contrato con esa clave en la empresa.");
            }

            var detalle = await GetByIdAsync(
                tipoContrato.Id,
                empresaId,
                ct);

            return ServiceResult<TipoContratoDetalleDto>
                .Ok(detalle!);
        }

        public async Task<ServiceResult<TipoContratoDetalleDto>> UpdateAsync(
            Guid id,
            TipoContratoGuardarDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default)
        {
            var tipoContrato = await context.TipoContratos
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId,
                    ct);

            if (tipoContrato is null)
            {
                return ServiceResult<TipoContratoDetalleDto>.Fail(
                    "Tipo de contrato no encontrado.");
            }

            var error = await ValidarDatosAsync(
                dto,
                empresaId,
                id,
                ct);

            if (error is not null)
            {
                return ServiceResult<TipoContratoDetalleDto>
                    .Fail(error);
            }

            MapearCampos(tipoContrato, dto);

            tipoContrato.FechaActualizacion = DateTime.UtcNow;
            tipoContrato.UsuarioModificacion = usuarioId;

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (EsDuplicado(ex))
            {
                return ServiceResult<TipoContratoDetalleDto>.Fail(
                    "Ya existe otro tipo de contrato con esa clave en la empresa.");
            }

            var detalle = await GetByIdAsync(
                tipoContrato.Id,
                empresaId,
                ct);

            return ServiceResult<TipoContratoDetalleDto>
                .Ok(detalle!);
        }

        public async Task<ServiceResult<bool>> UpdateStatusAsync(
            Guid id,
            Guid empresaId,
            bool activo,
            Guid usuarioId,
            CancellationToken ct = default)
        {
            var tipoContrato = await context.TipoContratos
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId,
                    ct);

            if (tipoContrato is null)
            {
                return ServiceResult<bool>.Fail(
                    "Tipo de contrato no encontrado.");
            }

            if (tipoContrato.Activo == activo)
            {
                return ServiceResult<bool>.Ok(true);
            }

            tipoContrato.Activo = activo;
            tipoContrato.FechaActualizacion = DateTime.UtcNow;
            tipoContrato.UsuarioModificacion = usuarioId;

            await context.SaveChangesAsync(ct);

            return ServiceResult<bool>.Ok(true);
        }

        private async Task<string?> ValidarDatosAsync(
            TipoContratoGuardarDto dto,
            Guid empresaId,
            Guid? excluirId,
            CancellationToken ct)
        {
            var clave = dto.Clave?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(clave))
                return "La clave es obligatoria.";

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return "El nombre es obligatorio.";

            var existe = await context.TipoContratos.AnyAsync(x =>
                x.EmpresaId == empresaId &&
                x.Clave == clave &&
                (!excluirId.HasValue ||
                    x.Id != excluirId.Value),
                ct);

            if (existe)
            {
                return "Ya existe un tipo de contrato con esa clave en la empresa.";
            }

            return null;
        }

        private static void MapearCampos(
            TipoContrato tipoContrato,
            TipoContratoGuardarDto dto)
        {
            tipoContrato.Clave = dto.Clave
                .Trim()
                .ToUpperInvariant();

            tipoContrato.Nombre = dto.Nombre.Trim();

            tipoContrato.Descripcion = Normalizar(
                dto.Descripcion);
        }

        private static string? Normalizar(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static bool EsDuplicado(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sql &&
                (sql.Number == 2601 || sql.Number == 2627);
        }
    }
}
