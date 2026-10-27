using Microsoft.AspNetCore.Identity;
using SIGED.api.Data;
using SIGED.api.Models.Domain;
using SIGED.api.Models.Dto.Administrador.Usuarios;
using SIGED.api.Models.Dto;
using SIGED.api.Services.Administrador.Interface;
using Microsoft.EntityFrameworkCore;

namespace SIGED.api.Services.Administrador.Implementation
{
    public class UsuarioAdministradorService : IUsuarioAdministradorService
    {
        private readonly SigedContext context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;

        public UsuarioAdministradorService(SigedContext context, UserManager<IdentityUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            this.context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public async Task<List<UsuarioListDto>> GetAllAsync()
        {
            var sistemaId = await GetSistemaIdAsync();

            var usuarios = await context.Usuarios.AsNoTracking()
                .Include(x => x.AspNetUser)
                .ThenInclude(x => x.Roles)
                .OrderBy(x => x.Nombre)
                .ThenBy(x => x.Apellidos)
                .ToListAsync();

            return usuarios.Select(x => new UsuarioListDto
            {
                Id = x.Id,
                AspNetUserId = x.AspNetUserId,
                Nombre = x.Nombre,
                Apellidos = x.Apellidos,
                Email = x.Email,
                Activo = x.Activo == true,
                FechaUltimoAcceso = x.FechaUltimoAcceso,
                Roles = x.AspNetUser.Roles
                    .Where(r => sistemaId.HasValue && r.SistemaId == sistemaId.Value && r.Name != null)
                    .Select(r => r.Name!)
                    .OrderBy(r => r)
                    .ToList()
            }).ToList();
        }

        public async Task<UsuarioDetalleDto?> GetByIdAsync(Guid id)
        {
            var sistemaId = await GetSistemaIdAsync();

            var usuario = await context.Usuarios.AsNoTracking()
                .Include(x => x.AspNetUser)
                .ThenInclude(x => x.Roles)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (usuario == null) return null;

            return MapDetalle(usuario, sistemaId);
        }

        public async Task<ServiceResult<UsuarioDetalleDto>> CreateAsync(UsuarioCreateDto dto)
        {
            var email = dto.Email.Trim().ToLower();

            if (await userManager.FindByEmailAsync(email) != null)
                return ServiceResult<UsuarioDetalleDto>.Fail("Ya existe un usuario con ese correo.");

            var rolesResult = await ValidarRolesAsync(dto.Roles);
            if (!rolesResult.Success) return ServiceResult<UsuarioDetalleDto>.Fail(rolesResult.Message!);

            var identityUser = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var identityResult = await userManager.CreateAsync(identityUser, dto.Password);

            if (!identityResult.Succeeded)
                return ServiceResult<UsuarioDetalleDto>.Fail(GetIdentityErrors(identityResult));

            try
            {
                if (rolesResult.Data!.Count > 0)
                {
                    var roleResult = await userManager.AddToRolesAsync(identityUser, rolesResult.Data);

                    if (!roleResult.Succeeded)
                    {
                        await userManager.DeleteAsync(identityUser);
                        return ServiceResult<UsuarioDetalleDto>.Fail(GetIdentityErrors(roleResult));
                    }
                }

                var usuario = new Usuario
                {
                    Id = Guid.NewGuid(),
                    AspNetUserId = identityUser.Id,
                    Nombre = dto.Nombre.Trim(),
                    Apellidos = dto.Apellidos.Trim(),
                    Email = email,
                    Telefono = Normalizar(dto.Telefono),
                    Celular = Normalizar(dto.Celular),
                    Pais = Normalizar(dto.Pais),
                    CodigoPais = Normalizar(dto.CodigoPais),
                    Empresa = Normalizar(dto.Empresa),
                    DominioPrincipal = NormalizarDominio(dto.DominioPrincipal),
                    EsAdministrador = rolesResult.Data.Any(x => x.Equals("ADMINISTRADOR", StringComparison.OrdinalIgnoreCase)),
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                context.Usuarios.Add(usuario);
                await context.SaveChangesAsync();

                var detalle = await GetByIdAsync(usuario.Id);
                return ServiceResult<UsuarioDetalleDto>.Ok(detalle!);
            }
            catch
            {
                await userManager.DeleteAsync(identityUser);
                throw;
            }
        }

        public async Task<ServiceResult<UsuarioDetalleDto>> UpdateAsync(Guid id, UsuarioUpdateDto dto)
        {
            var usuario = await context.Usuarios.FirstOrDefaultAsync(x => x.Id == id);

            if (usuario == null)
                return ServiceResult<UsuarioDetalleDto>.Fail("Usuario no encontrado.");

            var identityUser = await userManager.FindByIdAsync(usuario.AspNetUserId);

            if (identityUser == null)
                return ServiceResult<UsuarioDetalleDto>.Fail("El usuario no tiene una cuenta de acceso válida.");

            var email = dto.Email.Trim().ToLower();
            var existente = await userManager.FindByEmailAsync(email);

            if (existente != null && existente.Id != identityUser.Id)
                return ServiceResult<UsuarioDetalleDto>.Fail("Ya existe otro usuario con ese correo.");

            identityUser.Email = email;
            identityUser.UserName = email;

            var identityResult = await userManager.UpdateAsync(identityUser);

            if (!identityResult.Succeeded)
                return ServiceResult<UsuarioDetalleDto>.Fail(GetIdentityErrors(identityResult));

            usuario.Nombre = dto.Nombre.Trim();
            usuario.Apellidos = dto.Apellidos.Trim();
            usuario.Email = email;
            usuario.Telefono = Normalizar(dto.Telefono);
            usuario.Celular = Normalizar(dto.Celular);
            usuario.Pais = Normalizar(dto.Pais);
            usuario.CodigoPais = Normalizar(dto.CodigoPais);
            usuario.Empresa = Normalizar(dto.Empresa);
            usuario.DominioPrincipal = NormalizarDominio(dto.DominioPrincipal);
            usuario.FechaActualizacion = DateTime.UtcNow;

            await context.SaveChangesAsync();

            var detalle = await GetByIdAsync(id);
            return ServiceResult<UsuarioDetalleDto>.Ok(detalle!);
        }

        public async Task<ServiceResult<bool>> UpdateStatusAsync(Guid id, bool activo)
        {
            var usuario = await context.Usuarios.FirstOrDefaultAsync(x => x.Id == id);

            if (usuario == null)
                return ServiceResult<bool>.Fail("Usuario no encontrado.");

            usuario.Activo = activo;
            usuario.FechaActualizacion = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<UsuarioDetalleDto>> UpdateRolesAsync(Guid id, List<string> roles)
        {
            var usuario = await context.Usuarios.FirstOrDefaultAsync(x => x.Id == id);

            if (usuario == null)
                return ServiceResult<UsuarioDetalleDto>.Fail("Usuario no encontrado.");

            var identityUser = await userManager.FindByIdAsync(usuario.AspNetUserId);

            if (identityUser == null)
                return ServiceResult<UsuarioDetalleDto>.Fail("El usuario no tiene una cuenta de acceso válida.");

            var rolesResult = await ValidarRolesAsync(roles);

            if (!rolesResult.Success)
                return ServiceResult<UsuarioDetalleDto>.Fail(rolesResult.Message!);

            var sistemaId = await GetSistemaIdAsync();

            if (!sistemaId.HasValue)
                return ServiceResult<UsuarioDetalleDto>.Fail("No se encontró configurado el sistema SIGED.");

            var rolesSigedDisponibles = await roleManager.Roles.AsNoTracking()
                .Where(x => x.SistemaId == sistemaId.Value && x.Name != null)
                .Select(x => x.Name!)
                .ToListAsync();

            var rolesActuales = await userManager.GetRolesAsync(identityUser);

            var rolesActualesSiged = rolesActuales
                .Where(x => rolesSigedDisponibles.Contains(x, StringComparer.OrdinalIgnoreCase))
                .ToList();

            var remover = rolesActualesSiged
                .Where(x => !rolesResult.Data!.Contains(x, StringComparer.OrdinalIgnoreCase))
                .ToList();

            var agregar = rolesResult.Data!
                .Where(x => !rolesActuales.Contains(x, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (remover.Count > 0)
            {
                var result = await userManager.RemoveFromRolesAsync(identityUser, remover);

                if (!result.Succeeded)
                    return ServiceResult<UsuarioDetalleDto>.Fail(GetIdentityErrors(result));
            }

            if (agregar.Count > 0)
            {
                var result = await userManager.AddToRolesAsync(identityUser, agregar);

                if (!result.Succeeded)
                    return ServiceResult<UsuarioDetalleDto>.Fail(GetIdentityErrors(result));
            }

            usuario.EsAdministrador = rolesResult.Data.Any(x => x.Equals("ADMINISTRADOR", StringComparison.OrdinalIgnoreCase));
            usuario.FechaActualizacion = DateTime.UtcNow;

            await context.SaveChangesAsync();

            var detalle = await GetByIdAsync(id);
            return ServiceResult<UsuarioDetalleDto>.Ok(detalle!);
        }

        private async Task<int?> GetSistemaIdAsync()
        {
            return await context.AspNetSystems.AsNoTracking()
                .Where(x => x.Clave == "SIGED")
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();
        }

        private async Task<ServiceResult<List<string>>> ValidarRolesAsync(List<string> roles)
        {
            var sistemaId = await GetSistemaIdAsync();

            if (!sistemaId.HasValue)
                return ServiceResult<List<string>>.Fail("No se encontró configurado el sistema SIGED.");

            var disponibles = await roleManager.Roles.AsNoTracking()
                .Where(x => x.SistemaId == sistemaId.Value && x.Name != null)
                .Select(x => x.Name!)
                .ToListAsync();

            var lookup = disponibles.ToDictionary(x => x.ToUpperInvariant(), x => x);
            var solicitados = roles.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).Distinct().ToList();
            var invalidos = solicitados.Where(x => !lookup.ContainsKey(x)).ToList();

            if (invalidos.Count > 0)
                return ServiceResult<List<string>>.Fail($"Roles no válidos para SIGED: {string.Join(", ", invalidos)}.");

            return ServiceResult<List<string>>.Ok(solicitados.Select(x => lookup[x]).ToList());
        }

        private static UsuarioDetalleDto MapDetalle(Usuario usuario, int? sistemaId)
        {
            return new UsuarioDetalleDto
            {
                Id = usuario.Id,
                AspNetUserId = usuario.AspNetUserId,
                Nombre = usuario.Nombre,
                Apellidos = usuario.Apellidos,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                Celular = usuario.Celular,
                Pais = usuario.Pais,
                CodigoPais = usuario.CodigoPais,
                Empresa = usuario.Empresa,
                DominioPrincipal = usuario.DominioPrincipal,
                Activo = usuario.Activo == true,
                FechaUltimoAcceso = usuario.FechaUltimoAcceso,
                FechaCreacion = usuario.FechaCreacion,
                FechaActualizacion = usuario.FechaActualizacion,
                Roles = usuario.AspNetUser.Roles
                    .Where(x => sistemaId.HasValue && x.SistemaId == sistemaId.Value && x.Name != null)
                    .Select(x => x.Name!)
                    .OrderBy(x => x)
                    .ToList()
            };
        }

        private static string GetIdentityErrors(IdentityResult result)
        {
            return string.Join(" ", result.Errors.Select(x => x.Description));
        }

        private static string? Normalizar(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string? NormalizarDominio(string? dominio)
        {
            if (string.IsNullOrWhiteSpace(dominio)) return null;

            return dominio.Trim().ToLower()
                .Replace("https://", "")
                .Replace("http://", "")
                .Replace("www.", "")
                .Trim('/');
        }
    }
}
