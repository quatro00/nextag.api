using SIGED.api.Data;
using SIGED.api.Models.Domain;
using SIGED.api.Models.Dto.Administrador.Areas;
using SIGED.api.Models.Dto;
using SIGED.api.Services.Administrador.Interface;
using Microsoft.EntityFrameworkCore;

namespace SIGED.api.Services.Administrador.Implementation
{
    public class AreaAdministradorService : IAreaAdministradorService
    {
        private readonly SigedContext context;

        public AreaAdministradorService(SigedContext context)
        {
            this.context = context;
        }

        public async Task<List<AreaListDto>> GetAllAsync()
        {
            return await context.Areas.AsNoTracking()
                .OrderBy(x => x.Nombre)
                .Select(x => new AreaListDto
                {
                    Id = x.Id,
                    Clave = x.Clave,
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion,
                    Activo = x.Activo == true,
                    FechaCreacion = x.FechaCreacion
                })
                .ToListAsync();
        }

        public async Task<AreaDetalleDto?> GetByIdAsync(Guid id)
        {
            return await context.Areas.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new AreaDetalleDto
                {
                    Id = x.Id,
                    Clave = x.Clave,
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion,
                    Activo = x.Activo == true,
                    FechaCreacion = x.FechaCreacion,
                    FechaActualizacion = x.FechaActualizacion
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ServiceResult<AreaDetalleDto>> CreateAsync(AreaCreateDto dto)
        {
            var clave = NormalizarClave(dto.Clave);

            if (await context.Areas.AnyAsync(x => x.Clave == clave))
                return ServiceResult<AreaDetalleDto>.Fail("Ya existe un área con esa clave.");

            var area = new Area
            {
                Id = Guid.NewGuid(),
                Clave = clave,
                Nombre = dto.Nombre.Trim(),
                Descripcion = Normalizar(dto.Descripcion),
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            context.Areas.Add(area);
            await context.SaveChangesAsync();

            return ServiceResult<AreaDetalleDto>.Ok(MapDetalle(area));
        }

        public async Task<ServiceResult<AreaDetalleDto>> UpdateAsync(Guid id, AreaUpdateDto dto)
        {
            var area = await context.Areas.FirstOrDefaultAsync(x => x.Id == id);

            if (area == null)
                return ServiceResult<AreaDetalleDto>.Fail("Área no encontrada.");

            var clave = NormalizarClave(dto.Clave);

            if (await context.Areas.AnyAsync(x => x.Clave == clave && x.Id != id))
                return ServiceResult<AreaDetalleDto>.Fail("Ya existe otra área con esa clave.");

            area.Clave = clave;
            area.Nombre = dto.Nombre.Trim();
            area.Descripcion = Normalizar(dto.Descripcion);
            area.FechaActualizacion = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return ServiceResult<AreaDetalleDto>.Ok(MapDetalle(area));
        }

        public async Task<ServiceResult<bool>> UpdateStatusAsync(Guid id, bool activo)
        {
            var area = await context.Areas.FirstOrDefaultAsync(x => x.Id == id);

            if (area == null)
                return ServiceResult<bool>.Fail("Área no encontrada.");

            area.Activo = activo;
            area.FechaActualizacion = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true);
        }

        private static AreaDetalleDto MapDetalle(Area area)
        {
            return new AreaDetalleDto
            {
                Id = area.Id,
                Clave = area.Clave,
                Nombre = area.Nombre,
                Descripcion = area.Descripcion,
                Activo = area.Activo == true,
                FechaCreacion = area.FechaCreacion,
                FechaActualizacion = area.FechaActualizacion
            };
        }

        private static string NormalizarClave(string value)
        {
            return value.Trim().ToUpperInvariant();
        }

        private static string? Normalizar(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
