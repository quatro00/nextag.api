using Nextag.Api.Models.Dto.Tickets.Publicos;

namespace Nextag.Api.Services.Tickets.Interface
{
    public interface ITicketPublicoService
    {
        Task<TicketPublicoResultadoDto?> CrearAsync(
            string tokenPublico,
            TicketPublicoCrearDto dto,
            CancellationToken ct = default);

        Task<List<CategoriaTicketPublicaDto>?>
            GetCategoriasAsync(
                string tokenPublico,
                CancellationToken ct = default);
    }
}
