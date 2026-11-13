using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto.Etiquetas.Configuracion;
using Nextag.Api.Services.EtiquetaConfiguraciones.Interface;

namespace Nextag.Api.Services.EtiquetaConfiguraciones.Implementation
{
    public class EtiquetaConfiguracionService
        : IEtiquetaConfiguracionService
    {
        private readonly NextagContext context;

        public EtiquetaConfiguracionService(
            NextagContext context)
        {
            this.context = context;
        }

        // ==========================================
        // CONSULTAR CONFIGURACION
        // ==========================================

        public async Task<EtiquetaConfiguracionDto> GetAsync(
            Guid empresaId,
            CancellationToken ct = default)
        {
            var configuracion = await context.EtiquetaConfiguracions
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.EmpresaId == empresaId,
                    ct);

            if (configuracion == null)
            {
                return ObtenerPredeterminados(empresaId);
            }

            return Mapear(configuracion);
        }

        // ==========================================
        // GUARDAR CONFIGURACION
        // ==========================================

        public async Task<EtiquetaConfiguracionDto> GuardarAsync(
            Guid empresaId,
            Guid usuarioId,
            EtiquetaConfiguracionGuardarDto dto,
            CancellationToken ct = default)
        {
            var configuracion = await context.EtiquetaConfiguracions
                .FirstOrDefaultAsync(
                    x => x.EmpresaId == empresaId,
                    ct);

            var ahora = DateTime.UtcNow;

            if (configuracion == null)
            {
                // Crear configuración inicial.
                configuracion = new EtiquetaConfiguracion
                {
                    Id = Guid.NewGuid(),

                    EmpresaId = empresaId,

                    FechaCreacion = ahora,
                    UsuarioCreacion = usuarioId
                };

                context.EtiquetaConfiguracions.Add(
                    configuracion);
            }
            else
            {
                // Auditoría de actualización.
                configuracion.FechaActualizacion = ahora;
                configuracion.UsuarioModificacion = usuarioId;
            }

            // ======================================
            // CONFIGURACION GENERAL
            // ======================================

            configuracion.Activo =
                dto.Activo!.Value;

            configuracion.PermitirTicketPublico =
                dto.PermitirTicketPublico!.Value;

            // ======================================
            // INFORMACION DEL ACTIVO
            // ======================================

            configuracion.MostrarNombreActivo =
                dto.MostrarNombreActivo!.Value;

            configuracion.MostrarCodigoActivo =
                dto.MostrarCodigoActivo!.Value;

            configuracion.MostrarNumeroInventario =
                dto.MostrarNumeroInventario!.Value;

            configuracion.MostrarNumeroSerie =
                dto.MostrarNumeroSerie!.Value;

            configuracion.MostrarCategoria =
                dto.MostrarCategoria!.Value;

            configuracion.MostrarMarcaModelo =
                dto.MostrarMarcaModelo!.Value;

            configuracion.MostrarSucursal =
                dto.MostrarSucursal!.Value;

            configuracion.MostrarUbicacion =
                dto.MostrarUbicacion!.Value;

            configuracion.MostrarResponsable =
                dto.MostrarResponsable!.Value;

            configuracion.MostrarEstadoActivo =
                dto.MostrarEstadoActivo!.Value;

            // ======================================
            // EMPRESA
            // ======================================

            configuracion.MostrarDatosEmpresa =
                dto.MostrarDatosEmpresa!.Value;

            // ======================================
            // GUARDAR
            // ======================================

            await context.SaveChangesAsync(ct);

            return Mapear(configuracion);
        }

        // ==========================================
        // VALORES PREDETERMINADOS
        // ==========================================

        private static EtiquetaConfiguracionDto
            ObtenerPredeterminados(Guid empresaId)
        {
            return new EtiquetaConfiguracionDto
            {
                Id = null,

                EmpresaId = empresaId,

                Activo = true,

                // Deshabilitado por seguridad.
                PermitirTicketPublico = false,

                MostrarNombreActivo = true,

                MostrarCodigoActivo = false,

                MostrarNumeroInventario = false,

                MostrarNumeroSerie = false,

                MostrarCategoria = false,

                MostrarMarcaModelo = false,

                MostrarSucursal = false,

                MostrarUbicacion = false,

                MostrarResponsable = false,

                MostrarEstadoActivo = false,

                MostrarDatosEmpresa = false
            };
        }

        // ==========================================
        // MAPEO
        // ==========================================

        private static EtiquetaConfiguracionDto Mapear(
            EtiquetaConfiguracion configuracion)
        {
            return new EtiquetaConfiguracionDto
            {
                Id = configuracion.Id,

                EmpresaId = configuracion.EmpresaId,

                Activo = configuracion.Activo == true,

                PermitirTicketPublico =
                    configuracion.PermitirTicketPublico,

                MostrarNombreActivo =
                    configuracion.MostrarNombreActivo == true,

                MostrarCodigoActivo =
                    configuracion.MostrarCodigoActivo == true,

                MostrarNumeroInventario =
                    configuracion.MostrarNumeroInventario == true,

                MostrarNumeroSerie =
                    configuracion.MostrarNumeroSerie,

                MostrarCategoria =
                    configuracion.MostrarCategoria == true,

                MostrarMarcaModelo =
                    configuracion.MostrarMarcaModelo == true,

                MostrarSucursal =
                    configuracion.MostrarSucursal,

                MostrarUbicacion =
                    configuracion.MostrarUbicacion,

                MostrarResponsable =
                    configuracion.MostrarResponsable,

                MostrarEstadoActivo =
                    configuracion.MostrarEstadoActivo == true,

                MostrarDatosEmpresa =
                    configuracion.MostrarDatosEmpresa == true
            };
        }
    }
}
