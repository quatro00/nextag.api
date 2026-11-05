using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Dto.EstadosActivo;
using Nextag.Api.Services.EstadosActivo.Interface;

namespace Nextag.Api.Services.EstadosActivo.Implementation
{
    public class EstadoActivoService : IEstadoActivoService
    {
        private readonly NextagContext context;

        public EstadoActivoService(NextagContext context)
        {
            this.context = context;
        }

        // ==========================================
        // LISTADO DE ESTADOS ACTIVOS
        // ==========================================

        public async Task<List<EstadoActivoDto>> GetAllAsync(
            CancellationToken ct = default)
        {
            return await context.EstadoActivos
                .AsNoTracking()
                .Where(x => x.Activo == true)
                .OrderBy(x => x.Id)
                .Select(x => new EstadoActivoDto
                {
                    Id = x.Id,
                    Clave = x.Clave,
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion,
                    Activo = x.Activo == true
                })
                .ToListAsync(ct);
        }

        // ==========================================
        // CONSULTAR POR ID
        // ==========================================

        public async Task<EstadoActivoDto?> GetByIdAsync(
            int id,
            CancellationToken ct = default)
        {
            return await context.EstadoActivos
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new EstadoActivoDto
                {
                    Id = x.Id,
                    Clave = x.Clave,
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion,
                    Activo = x.Activo == true
                })
                .FirstOrDefaultAsync(ct);
        }
    }
}
