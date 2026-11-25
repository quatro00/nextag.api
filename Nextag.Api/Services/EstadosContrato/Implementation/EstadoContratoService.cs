using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Dto.EstadosContrato;
using Nextag.Api.Services.EstadosContrato.Interface;

namespace Nextag.Api.Services.EstadosContrato.Implementation
{
    public class EstadoContratoService : IEstadoContratoService
    {
        private readonly NextagContext context;

        public EstadoContratoService(NextagContext context)
        {
            this.context = context;
        }

        public Task<List<EstadoContratoDto>> GetAllAsync(
            bool? activo = null,
            CancellationToken ct = default)
        {
            var query = context.EstadoContratos
                .AsNoTracking()
                .AsQueryable();

            if (activo.HasValue)
            {
                if (activo.Value)
                    query = query.Where(x => x.Activo == true);
                else
                    query = query.Where(x => x.Activo != true);
            }

            return query
                .OrderBy(x => x.Orden)
                .ThenBy(x => x.Id)
                .Select(x => new EstadoContratoDto
                {
                    Id = x.Id,
                    Clave = x.Clave,
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion,
                    Activo = x.Activo == true,
                    Orden = x.Orden
                })
                .ToListAsync(ct);
        }
    }
}
