using Nextag.Api.Models.Dto.ActivoAsignaciones;
using Nextag.Api.Models.Dto;

namespace Nextag.Api.Services.ActivoAsignaciones.Interface
{
    public interface IActivoAsignacionService
    {
        // =====================================
        // CONSULTAS
        // =====================================

        Task<ActivoAsignacionPaginaDto> GetAllAsync(
            Guid empresaId,
            ActivoAsignacionFiltroDto filtros,
            CancellationToken ct = default);

        Task<ActivoAsignacionDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default);

        Task<List<ActivoAsignacionDto>> GetHistorialActivoAsync(
            Guid activoId,
            Guid empresaId,
            CancellationToken ct = default);

        Task<List<ActivoAsignacionDto>> GetHistorialResponsableAsync(
            Guid responsableId,
            Guid empresaId,
            CancellationToken ct = default);

        Task<ActivoAsignacionDto?> GetVigenteActivoAsync(
            Guid activoId,
            Guid empresaId,
            CancellationToken ct = default);

        Task<ActivosSinAsignarPaginaDto> GetActivosSinAsignarAsync(
            Guid empresaId,
            string? buscar,
            Guid? sucursalId,
            int pagina,
            int tamanoPagina,
            CancellationToken ct = default);

        // =====================================
        // OPERACIONES
        // =====================================

        Task<ServiceResult<ActivoAsignacionDto>> CreateAsync(
            ActivoAsignacionCrearDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);

        Task<ServiceResult<ActivoAsignacionDto>> DevolverAsync(
            Guid id,
            ActivoAsignacionDevolverDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);

        Task<ServiceResult<ActivoAsignacionDto>> ReasignarAsync(
            Guid id,
            ActivoAsignacionReasignarDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default);
    }
}
