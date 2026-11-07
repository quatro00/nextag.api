using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto.Etiquetas.Publicas;
using Nextag.Api.Services.EtiquetasPublicas.Interface;

namespace Nextag.Api.Services.EtiquetasPublicas.Implementation
{
    public class EtiquetaPublicaService : IEtiquetaPublicaService
    {
        private readonly NextagContext context;
        private readonly ILogger<EtiquetaPublicaService> logger;

        public EtiquetaPublicaService(
            NextagContext context,
            ILogger<EtiquetaPublicaService> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<EtiquetaPublicaDto?> ConsultarAsync(
            string tokenPublico,
            string? tipoLectura,
            string? ip,
            string? userAgent,
            CancellationToken ct = default)
        {
            // 1. Normalizar y validar el token.
            tokenPublico = tokenPublico?.Trim() ?? string.Empty;

            // Los tokens actuales se generan como 64 caracteres hexadecimales.
            if (tokenPublico.Length != 64 ||
                !tokenPublico.All(Uri.IsHexDigit))
            {
                return null;
            }

            tokenPublico = tokenPublico.ToLowerInvariant();

            // 2. Obtener etiqueta y empresa.
            var etiqueta = await context.Etiqueta
                .AsNoTracking()
                .Where(e => e.TokenPublico == tokenPublico)
                .Select(e => new
                {
                    e.Id,
                    e.EmpresaId,
                    e.Codigo,
                    e.Anulada,
                    e.PublicaHabilitada,

                    EmpresaActiva = e.Empresa.Activo,
                    EmpresaNombre = e.Empresa.Nombre,
                    EmpresaLogo = e.Empresa.LogoUrl
                })
                .FirstOrDefaultAsync(ct);

            if (etiqueta is null)
                return null;

            // 3. Validar acceso público.
            if (etiqueta.Anulada ||
                etiqueta.PublicaHabilitada != true ||
                etiqueta.EmpresaActiva != true)
            {
                return null;
            }

            // 4. Obtener configuración de la empresa.
            var configuracion = await context.EtiquetaConfiguracions
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.EmpresaId == etiqueta.EmpresaId,
                    ct);

            // Una configuración expresamente inactiva bloquea la ficha.
            if (configuracion is not null &&
                configuracion.Activo != true)
            {
                return null;
            }

            // 5. Crear respuesta.
            var respuesta = new EtiquetaPublicaDto
            {
                CodigoEtiqueta = etiqueta.Codigo,

                Estatus = "SIN_ASIGNAR",

                Mensaje = "La etiqueta no tiene un activo asignado.",

                PermitirTicketPublico = false
            };

            // 6. Datos públicos de la empresa.
            // Si no existe configuración, no exponemos estos datos.
            if (configuracion?.MostrarDatosEmpresa == true)
            {
                respuesta.Empresa = new EmpresaPublicaDto
                {
                    Nombre = etiqueta.EmpresaNombre,
                    LogoUrl = etiqueta.EmpresaLogo
                };
            }

            // 7. Obtener vinculación vigente.
            var vinculo = await context.ActivoEtiqueta
                .AsNoTracking()
                .Where(x =>
                    x.EmpresaId == etiqueta.EmpresaId &&
                    x.EtiquetaId == etiqueta.Id &&
                    x.FechaDesvinculacion == null)
                .Select(x => new
                {
                    x.ActivoId
                })
                .FirstOrDefaultAsync(ct);

            if (vinculo is null)
            {
                await RegistrarLecturaAsync(
                    etiqueta.Id,
                    etiqueta.EmpresaId,
                    null,
                    tipoLectura,
                    ip,
                    userAgent,
                    "SIN_ASIGNAR",
                    ct);

                return respuesta;
            }

            // 8. Obtener información del activo.
            var activo = await context.Activos
                .AsNoTracking()
                .Where(a =>
                    a.Id == vinculo.ActivoId &&
                    a.EmpresaId == etiqueta.EmpresaId)
                .Select(a => new
                {
                    a.Id,
                    a.Nombre,
                    a.Codigo,
                    a.NumeroInventario,
                    a.NumeroSerie,

                    Categoria = a.CategoriaActivo.Nombre,

                    Marca = a.Marca != null
                        ? a.Marca.Nombre
                        : null,

                    Modelo = a.ModeloActivo != null
                        ? a.ModeloActivo.Nombre
                        : null,

                    Sucursal = a.Sucursal.Nombre,

                    Ubicacion = a.Ubicacion != null
                        ? a.Ubicacion.Nombre
                        : null,

                    Estado = a.EstadoActivo.Nombre
                })
                .FirstOrDefaultAsync(ct);

            if (activo is null)
            {
                // Situación inconsistente: existe vínculo, pero no activo.
                // No exponer detalles internos al público.
                logger.LogWarning(
                    "Vinculación sin activo válido. EtiquetaId: {EtiquetaId}",
                    etiqueta.Id);

                return null;
            }

            // 9. Configuración de visualización.
            // Sin configuración aplicamos una política conservadora.
            bool mostrarNombre =
                configuracion?.MostrarNombreActivo == true ||
                configuracion is null;

            bool mostrarCodigo =
                configuracion?.MostrarCodigoActivo == true;

            bool mostrarInventario =
                configuracion?.MostrarNumeroInventario == true;

            bool mostrarSerie =
                configuracion?.MostrarNumeroSerie == true;

            bool mostrarCategoria =
                configuracion?.MostrarCategoria == true;

            bool mostrarMarcaModelo =
                configuracion?.MostrarMarcaModelo == true;

            bool mostrarSucursal =
                configuracion?.MostrarSucursal == true;

            bool mostrarUbicacion =
                configuracion?.MostrarUbicacion == true;

            bool mostrarEstado =
                configuracion?.MostrarEstadoActivo == true;

            // 10. Construir ficha pública.
            respuesta.Activo = new ActivoPublicoDto
            {
                Nombre = mostrarNombre
                    ? activo.Nombre
                    : null,

                Codigo = mostrarCodigo
                    ? activo.Codigo
                    : null,

                NumeroInventario = mostrarInventario
                    ? activo.NumeroInventario
                    : null,

                NumeroSerie = mostrarSerie
                    ? activo.NumeroSerie
                    : null,

                Categoria = mostrarCategoria
                    ? activo.Categoria
                    : null,

                Marca = mostrarMarcaModelo
                    ? activo.Marca
                    : null,

                Modelo = mostrarMarcaModelo
                    ? activo.Modelo
                    : null,

                Sucursal = mostrarSucursal
                    ? activo.Sucursal
                    : null,

                Ubicacion = mostrarUbicacion
                    ? activo.Ubicacion
                    : null,

                Estado = mostrarEstado
                    ? activo.Estado
                    : null
            };

            respuesta.Estatus = "ACTIVO";
            respuesta.Mensaje = "Activo encontrado.";

            // Preparado para cuando implementemos tickets.
            respuesta.PermitirTicketPublico =
                configuracion?.PermitirTicketPublico == true;

            // 11. Registrar lectura.
            await RegistrarLecturaAsync(
                etiqueta.Id,
                etiqueta.EmpresaId,
                activo.Id,
                tipoLectura,
                ip,
                userAgent,
                "ACTIVO",
                ct);

            return respuesta;
        }

        private async Task RegistrarLecturaAsync(
            Guid etiquetaId,
            Guid empresaId,
            Guid? activoId,
            string? tipoLectura,
            string? ip,
            string? userAgent,
            string resultado,
            CancellationToken ct)
        {
            // Normalizar tipo de lectura.
            var tipo = tipoLectura?.Trim().ToUpperInvariant();

            if (tipo is not ("NFC" or "QR"))
            {
                tipo = "DESCONOCIDO";
            }

            var lectura = new EtiquetaLectura
            {
                EmpresaId = empresaId,
                EtiquetaId = etiquetaId,
                ActivoId = activoId,

                TipoLectura = tipo,

                FechaLectura = DateTime.UtcNow,

                Ip = string.IsNullOrWhiteSpace(ip)
                    ? null
                    : ip.Length > 45 ? ip[..45] : ip,

                UserAgent = string.IsNullOrWhiteSpace(userAgent)
                    ? null
                    : userAgent.Length > 500
                        ? userAgent[..500]
                        : userAgent,

                Resultado = resultado
            };

            try
            {
                context.EtiquetaLecturas.Add(lectura);

                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                // Un error de estadísticas no debe impedir consultar el activo.
                logger.LogError(
                    ex,
                    "Error al registrar lectura de etiqueta {EtiquetaId}",
                    etiquetaId);

                context.Entry(lectura).State = EntityState.Detached;
            }
        }
    }
}
