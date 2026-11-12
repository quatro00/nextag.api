namespace Nextag.Api.Services.Tickets.Interface
{
    public interface ITicketFolioService
    {
        Task<long> SiguienteAsync(
            Guid empresaId,
            CancellationToken ct = default);
    }
}
