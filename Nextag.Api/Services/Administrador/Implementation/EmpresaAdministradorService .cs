using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto.Administrador.Empresas;
using Nextag.Api.Models.Dto;
using Nextag.Api.Services.Administrador.Interface;

namespace Nextag.Api.Services.Administrador.Implementation
{
    public class EmpresaAdministradorService : IEmpresaAdministradorService
    {
        private readonly NextagContext context;

        public EmpresaAdministradorService(NextagContext context)
        {
            this.context = context;
        }

        // ==========================================
        // LISTADO
        // ==========================================

        public async Task<List<EmpresaListDto>> GetAllAsync()
        {
            return await context.Empresas
                .AsNoTracking()
                .OrderBy(x => x.Nombre)
                .Select(x => new EmpresaListDto
                {
                    Id = x.Id,
                    Clave = x.Clave,
                    Nombre = x.Nombre,
                    RazonSocial = x.RazonSocial,
                    Rfc = x.Rfc,
                    Correo = x.Correo,
                    Activo = x.Activo == true,
                    FechaCreacion = x.FechaCreacion
                })
                .ToListAsync();
        }

        // ==========================================
        // DETALLE
        // ==========================================

        public async Task<EmpresaDetalleDto?> GetByIdAsync(Guid id)
        {
            return await context.Empresas
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new EmpresaDetalleDto
                {
                    Id = x.Id,
                    Clave = x.Clave,
                    Nombre = x.Nombre,
                    RazonSocial = x.RazonSocial,
                    Rfc = x.Rfc,
                    Correo = x.Correo,
                    Telefono = x.Telefono,
                    SitioWeb = x.SitioWeb,
                    LogoUrl = x.LogoUrl,
                    Activo = x.Activo == true,
                    FechaCreacion = x.FechaCreacion,
                    UsuarioCreacion = x.UsuarioCreacion,
                    FechaActualizacion = x.FechaActualizacion,
                    UsuarioModificacion = x.UsuarioModificacion
                })
                .FirstOrDefaultAsync();
        }

        // ==========================================
        // CREAR
        // ==========================================

        public async Task<ServiceResult<EmpresaDetalleDto>> CreateAsync(EmpresaCreateDto dto, Guid usuarioId)
        {
            var clave = dto.Clave.Trim().ToUpperInvariant();

            var existe = await context.Empresas.AnyAsync(x => x.Clave == clave);

            if (existe)
            {
                return ServiceResult<EmpresaDetalleDto>.Fail("Ya existe una empresa con esa clave.");
            }

            var empresa = new Empresa
            {
                Clave = clave,
                Nombre = dto.Nombre.Trim(),
                RazonSocial = Normalizar(dto.RazonSocial),
                Rfc = Normalizar(dto.Rfc),
                Correo = Normalizar(dto.Correo),
                Telefono = Normalizar(dto.Telefono),
                SitioWeb = Normalizar(dto.SitioWeb),
                LogoUrl = Normalizar(dto.LogoUrl),

                Activo = true,

                UsuarioCreacion = usuarioId
            };

            context.Empresas.Add(empresa);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (EsClaveDuplicada(ex))
            {
                return ServiceResult<EmpresaDetalleDto>.Fail("Ya existe una empresa con esa clave.");
            }

            var detalle = await GetByIdAsync(empresa.Id);

            return ServiceResult<EmpresaDetalleDto>.Ok(detalle!);
        }

        // ==========================================
        // ACTUALIZAR
        // ==========================================

        public async Task<ServiceResult<EmpresaDetalleDto>> UpdateAsync(Guid id, EmpresaUpdateDto dto, Guid usuarioId)
        {
            var empresa = await context.Empresas.FirstOrDefaultAsync(x => x.Id == id);

            if (empresa == null)
            {
                return ServiceResult<EmpresaDetalleDto>.Fail("Empresa no encontrada.");
            }

            var clave = dto.Clave.Trim().ToUpperInvariant();

            var existe = await context.Empresas
                .AnyAsync(x =>
                    x.Clave == clave &&
                    x.Id != id
                );

            if (existe)
            {
                return ServiceResult<EmpresaDetalleDto>.Fail("Ya existe otra empresa con esa clave.");
            }

            empresa.Clave = clave;
            empresa.Nombre = dto.Nombre.Trim();
            empresa.RazonSocial = Normalizar(dto.RazonSocial);
            empresa.Rfc = Normalizar(dto.Rfc);
            empresa.Correo = Normalizar(dto.Correo);
            empresa.Telefono = Normalizar(dto.Telefono);
            empresa.SitioWeb = Normalizar(dto.SitioWeb);
            empresa.LogoUrl = Normalizar(dto.LogoUrl);

            empresa.FechaActualizacion = DateTime.UtcNow;
            empresa.UsuarioModificacion = usuarioId;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (EsClaveDuplicada(ex))
            {
                return ServiceResult<EmpresaDetalleDto>.Fail("Ya existe otra empresa con esa clave.");
            }

            var detalle = await GetByIdAsync(id);

            return ServiceResult<EmpresaDetalleDto>.Ok(detalle!);
        }

        // ==========================================
        // ACTIVAR / DESACTIVAR
        // ==========================================

        public async Task<ServiceResult<bool>> UpdateStatusAsync(Guid id, bool activo, Guid usuarioId)
        {
            var empresa = await context.Empresas.FirstOrDefaultAsync(x => x.Id == id);

            if (empresa == null)
            {
                return ServiceResult<bool>.Fail("Empresa no encontrada.");
            }

            empresa.Activo = activo;

            empresa.FechaActualizacion = DateTime.UtcNow;
            empresa.UsuarioModificacion = usuarioId;

            await context.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true);
        }

        // ==========================================
        // HELPERS
        // ==========================================

        private static string? Normalizar(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static bool EsClaveDuplicada(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
        }
    }
}
