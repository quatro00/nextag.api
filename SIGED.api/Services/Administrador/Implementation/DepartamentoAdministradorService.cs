using SIGED.api.Data;
using SIGED.api.Models.Domain;
using SIGED.api.Models.Dto.Administrador.Departamentos;
using SIGED.api.Models.Dto;
using SIGED.api.Services.Administrador.Interface;
using Microsoft.EntityFrameworkCore;

namespace SIGED.api.Services.Administrador.Implementation
{
    public class DepartamentoAdministradorService : IDepartamentoAdministradorService
    {
        private readonly SigedContext context;

        public DepartamentoAdministradorService(SigedContext context)
        {
            this.context = context;
        }

        public async Task<List<DepartamentoListDto>> GetAllAsync(Guid? areaId = null)
        {
            var query = context.Departamentos.AsNoTracking().AsQueryable();

            if (areaId.HasValue)
                query = query.Where(x => x.AreaId == areaId.Value);

            return await query
                .OrderBy(x => x.Area.Nombre)
                .ThenBy(x => x.Nombre)
                .Select(x => new DepartamentoListDto
                {
                    Id = x.Id,
                    AreaId = x.AreaId,
                    AreaClave = x.Area.Clave,
                    AreaNombre = x.Area.Nombre,
                    Clave = x.Clave,
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion,
                    Activo = x.Activo == true,
                    FechaCreacion = x.FechaCreacion
                })
                .ToListAsync();
        }

        public async Task<DepartamentoDetalleDto?> GetByIdAsync(Guid id)
        {
            return await context.Departamentos.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new DepartamentoDetalleDto
                {
                    Id = x.Id,
                    AreaId = x.AreaId,
                    AreaClave = x.Area.Clave,
                    AreaNombre = x.Area.Nombre,
                    Clave = x.Clave,
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion,
                    Activo = x.Activo == true,
                    FechaCreacion = x.FechaCreacion,
                    FechaActualizacion = x.FechaActualizacion,
                    UsuarioCreacionId = x.UsuarioCreacionId,
                    UsuarioActualizacionId = x.UsuarioActualizacionId
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ServiceResult<DepartamentoDetalleDto>> CreateAsync(DepartamentoCreateDto dto, Guid usuarioId)
        {
            if (dto.AreaId == Guid.Empty)
                return ServiceResult<DepartamentoDetalleDto>.Fail("El área es requerida.");

            var area = await context.Areas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.AreaId);

            if (area == null)
                return ServiceResult<DepartamentoDetalleDto>.Fail("Área no encontrada.");

            if (area.Activo != true)
                return ServiceResult<DepartamentoDetalleDto>.Fail("No se puede agregar un departamento a un área inactiva.");

            var clave = NormalizarClave(dto.Clave);
            var nombre = dto.Nombre.Trim();

            if (await context.Departamentos.AnyAsync(x => x.Clave == clave))
                return ServiceResult<DepartamentoDetalleDto>.Fail("Ya existe un departamento con esa clave.");

            if (await context.Departamentos.AnyAsync(x => x.AreaId == dto.AreaId && x.Nombre == nombre))
                return ServiceResult<DepartamentoDetalleDto>.Fail("Ya existe un departamento con ese nombre dentro del área.");

            var departamento = new Departamento
            {
                AreaId = dto.AreaId,
                Clave = clave,
                Nombre = nombre,
                Descripcion = Normalizar(dto.Descripcion),
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                UsuarioCreacionId = usuarioId
            };

            context.Departamentos.Add(departamento);
            await context.SaveChangesAsync();

            var detalle = await GetByIdAsync(departamento.Id);
            return ServiceResult<DepartamentoDetalleDto>.Ok(detalle!);
        }

        public async Task<ServiceResult<DepartamentoDetalleDto>> UpdateAsync(Guid id, DepartamentoUpdateDto dto, Guid usuarioId)
        {
            var departamento = await context.Departamentos.FirstOrDefaultAsync(x => x.Id == id);

            if (departamento == null)
                return ServiceResult<DepartamentoDetalleDto>.Fail("Departamento no encontrado.");

            if (dto.AreaId == Guid.Empty)
                return ServiceResult<DepartamentoDetalleDto>.Fail("El área es requerida.");

            var area = await context.Areas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.AreaId);

            if (area == null)
                return ServiceResult<DepartamentoDetalleDto>.Fail("Área no encontrada.");

            if (departamento.AreaId != dto.AreaId && area.Activo != true)
                return ServiceResult<DepartamentoDetalleDto>.Fail("No se puede mover el departamento a un área inactiva.");

            var clave = NormalizarClave(dto.Clave);
            var nombre = dto.Nombre.Trim();

            if (await context.Departamentos.AnyAsync(x => x.Clave == clave && x.Id != id))
                return ServiceResult<DepartamentoDetalleDto>.Fail("Ya existe otro departamento con esa clave.");

            if (await context.Departamentos.AnyAsync(x => x.AreaId == dto.AreaId && x.Nombre == nombre && x.Id != id))
                return ServiceResult<DepartamentoDetalleDto>.Fail("Ya existe otro departamento con ese nombre dentro del área.");

            departamento.AreaId = dto.AreaId;
            departamento.Clave = clave;
            departamento.Nombre = nombre;
            departamento.Descripcion = Normalizar(dto.Descripcion);
            departamento.FechaActualizacion = DateTime.UtcNow;
            departamento.UsuarioActualizacionId = usuarioId;

            await context.SaveChangesAsync();

            var detalle = await GetByIdAsync(id);
            return ServiceResult<DepartamentoDetalleDto>.Ok(detalle!);
        }

        public async Task<ServiceResult<bool>> UpdateStatusAsync(Guid id, bool activo, Guid usuarioId)
        {
            var departamento = await context.Departamentos
                .Include(x => x.Area)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (departamento == null)
                return ServiceResult<bool>.Fail("Departamento no encontrado.");

            if (activo && departamento.Area.Activo != true)
                return ServiceResult<bool>.Fail("No se puede activar un departamento perteneciente a un área inactiva.");

            departamento.Activo = activo;
            departamento.FechaActualizacion = DateTime.UtcNow;
            departamento.UsuarioActualizacionId = usuarioId;

            await context.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true);
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
