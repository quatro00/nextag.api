using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Dto.TiposProveedor;
using Nextag.Api.Services.TiposProveedor.Interface;

namespace Nextag.Api.Services.TiposProveedor.Implementation;

public class TipoProveedorService : ITipoProveedorService
{
    private readonly NextagContext context;
    public TipoProveedorService(NextagContext context) => this.context = context;

    public Task<List<TipoProveedorDto>> GetAllAsync(CancellationToken ct = default)
    {
        return context.TipoProveedors.AsNoTracking()
            .Where(x => x.Activo == true)
            .OrderBy(x => x.Id)
            .Select(x => new TipoProveedorDto
            {
                Id = x.Id,
                Clave = x.Clave,
                Nombre = x.Nombre,
                Descripcion = x.Descripcion,
                Activo = x.Activo == true
            }).ToListAsync(ct);
    }
}
