using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto.Clientes;
using Nextag.Api.Models.Dto;
using Nextag.Api.Services.Clientes.Interface;

namespace Nextag.Api.Services.Clientes.Implementation
{
    public class ClienteService : IClienteService
    {
        private readonly NextagContext context;

        public ClienteService(NextagContext context)
        {
            this.context = context;
        }

        public Task<List<ClienteListDto>> GetAllAsync(
            Guid empresaId,
            bool? activo = null,
            string? buscar = null,
            CancellationToken ct = default)
        {
            var query = context.Clientes
                .AsNoTracking()
                .Where(x => x.EmpresaId == empresaId);

            if (activo.HasValue)
            {
                if (activo.Value)
                    query = query.Where(x => x.Activo == true);
                else
                    query = query.Where(x => x.Activo != true);
            }

            buscar = buscar?.Trim();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                query = query.Where(x =>
                    x.Clave.Contains(buscar) ||
                    x.Nombre.Contains(buscar) ||
                    (x.RazonSocial != null &&
                        x.RazonSocial.Contains(buscar)) ||
                    (x.Rfc != null &&
                        x.Rfc.Contains(buscar)) ||
                    (x.ContactoPrincipal != null &&
                        x.ContactoPrincipal.Contains(buscar)) ||
                    (x.Correo != null &&
                        x.Correo.Contains(buscar)) ||
                    (x.CorreoContacto != null &&
                        x.CorreoContacto.Contains(buscar)));
            }

            return query
                .OrderBy(x => x.Nombre)
                .ThenBy(x => x.Clave)
                .Select(x => new ClienteListDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,
                    Clave = x.Clave,
                    Nombre = x.Nombre,
                    RazonSocial = x.RazonSocial,
                    Rfc = x.Rfc,
                    Correo = x.Correo,
                    Telefono = x.Telefono,
                    ContactoPrincipal = x.ContactoPrincipal,
                    TelefonoContacto = x.TelefonoContacto,
                    CorreoContacto = x.CorreoContacto,
                    Activo = x.Activo == true
                })
                .ToListAsync(ct);
        }

        public Task<ClienteDetalleDto?> GetByIdAsync(
            Guid id,
            Guid empresaId,
            CancellationToken ct = default)
        {
            return context.Clientes
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId)
                .Select(x => new ClienteDetalleDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,
                    Clave = x.Clave,
                    Nombre = x.Nombre,
                    RazonSocial = x.RazonSocial,
                    Rfc = x.Rfc,
                    Correo = x.Correo,
                    Telefono = x.Telefono,
                    SitioWeb = x.SitioWeb,
                    Direccion = x.Direccion,
                    Colonia = x.Colonia,
                    Municipio = x.Municipio,
                    Estado = x.Estado,
                    Pais = x.Pais,
                    CodigoPostal = x.CodigoPostal,
                    ContactoPrincipal = x.ContactoPrincipal,
                    TelefonoContacto = x.TelefonoContacto,
                    CorreoContacto = x.CorreoContacto,
                    Observaciones = x.Observaciones,
                    Activo = x.Activo == true,
                    FechaCreacion = x.FechaCreacion,
                    UsuarioCreacion = x.UsuarioCreacion,
                    FechaActualizacion = x.FechaActualizacion,
                    UsuarioModificacion = x.UsuarioModificacion
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<ServiceResult<ClienteDetalleDto>> CreateAsync(
            ClienteGuardarDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default)
        {
            var error = await ValidarDatosAsync(
                dto,
                empresaId,
                null,
                ct);

            if (error is not null)
                return ServiceResult<ClienteDetalleDto>.Fail(error);

            var cliente = new Cliente
            {
                EmpresaId = empresaId,
                Activo = true,
                UsuarioCreacion = usuarioId,
                FechaCreacion = DateTime.UtcNow
            };

            MapearCampos(cliente, dto);

            try
            {
                context.Clientes.Add(cliente);

                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (EsDuplicado(ex))
            {
                return ServiceResult<ClienteDetalleDto>.Fail(
                    "Ya existe un cliente con esa clave en la empresa.");
            }

            var detalle = await GetByIdAsync(
                cliente.Id,
                empresaId,
                ct);

            return ServiceResult<ClienteDetalleDto>.Ok(detalle!);
        }

        public async Task<ServiceResult<ClienteDetalleDto>> UpdateAsync(
            Guid id,
            ClienteGuardarDto dto,
            Guid empresaId,
            Guid usuarioId,
            CancellationToken ct = default)
        {
            var cliente = await context.Clientes
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId,
                    ct);

            if (cliente is null)
            {
                return ServiceResult<ClienteDetalleDto>.Fail(
                    "Cliente no encontrado.");
            }

            var error = await ValidarDatosAsync(
                dto,
                empresaId,
                id,
                ct);

            if (error is not null)
                return ServiceResult<ClienteDetalleDto>.Fail(error);

            MapearCampos(cliente, dto);

            cliente.FechaActualizacion = DateTime.UtcNow;
            cliente.UsuarioModificacion = usuarioId;

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (EsDuplicado(ex))
            {
                return ServiceResult<ClienteDetalleDto>.Fail(
                    "Ya existe otro cliente con esa clave en la empresa.");
            }

            var detalle = await GetByIdAsync(
                id,
                empresaId,
                ct);

            return ServiceResult<ClienteDetalleDto>.Ok(detalle!);
        }

        public async Task<ServiceResult<bool>> UpdateStatusAsync(
            Guid id,
            Guid empresaId,
            bool activo,
            Guid usuarioId,
            CancellationToken ct = default)
        {
            var cliente = await context.Clientes
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId,
                    ct);

            if (cliente is null)
            {
                return ServiceResult<bool>.Fail(
                    "Cliente no encontrado.");
            }

            if (cliente.Activo == activo)
                return ServiceResult<bool>.Ok(true);

            cliente.Activo = activo;
            cliente.FechaActualizacion = DateTime.UtcNow;
            cliente.UsuarioModificacion = usuarioId;

            await context.SaveChangesAsync(ct);

            return ServiceResult<bool>.Ok(true);
        }

        private async Task<string?> ValidarDatosAsync(
            ClienteGuardarDto dto,
            Guid empresaId,
            Guid? excluirId,
            CancellationToken ct)
        {
            var clave = dto.Clave?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(clave))
                return "La clave es obligatoria.";

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return "El nombre es obligatorio.";

            var existe = await context.Clientes.AnyAsync(x =>
                x.EmpresaId == empresaId &&
                x.Clave == clave &&
                (!excluirId.HasValue ||
                    x.Id != excluirId.Value),
                ct);

            if (existe)
            {
                return "Ya existe un cliente con esa clave en la empresa.";
            }

            return null;
        }

        private static void MapearCampos(
            Cliente cliente,
            ClienteGuardarDto dto)
        {
            cliente.Clave = dto.Clave.Trim().ToUpperInvariant();
            cliente.Nombre = dto.Nombre.Trim();
            cliente.RazonSocial = Normalizar(dto.RazonSocial);
            cliente.Rfc = Normalizar(dto.Rfc)?.ToUpperInvariant();
            cliente.Correo = Normalizar(dto.Correo);
            cliente.Telefono = Normalizar(dto.Telefono);
            cliente.SitioWeb = Normalizar(dto.SitioWeb);
            cliente.Direccion = Normalizar(dto.Direccion);
            cliente.Colonia = Normalizar(dto.Colonia);
            cliente.Municipio = Normalizar(dto.Municipio);
            cliente.Estado = Normalizar(dto.Estado);
            cliente.Pais = Normalizar(dto.Pais);
            cliente.CodigoPostal = Normalizar(dto.CodigoPostal);
            cliente.ContactoPrincipal = Normalizar(dto.ContactoPrincipal);
            cliente.TelefonoContacto = Normalizar(dto.TelefonoContacto);
            cliente.CorreoContacto = Normalizar(dto.CorreoContacto);
            cliente.Observaciones = Normalizar(dto.Observaciones);
        }

        private static string? Normalizar(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static bool EsDuplicado(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sql &&
                (sql.Number == 2601 || sql.Number == 2627);
        }
    }
}
