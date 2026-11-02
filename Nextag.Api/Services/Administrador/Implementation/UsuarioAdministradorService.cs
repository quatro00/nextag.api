using Microsoft.AspNetCore.Identity;
using Nextag.Api.Data;
using Nextag.Api.Models.Domain;
using Nextag.Api.Models.Dto.Administrador.Usuarios;
using Nextag.Api.Models.Dto;
using Nextag.Api.Services.Administrador.Interface;
using Microsoft.EntityFrameworkCore;

namespace Nextag.Api.Services.Administrador.Implementation
{
    public class UsuarioAdministradorService : IUsuarioAdministradorService
    {
        private readonly NextagContext context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;

        public UsuarioAdministradorService(NextagContext context, UserManager<IdentityUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            this.context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public async Task<List<UsuarioListDto>> GetAllAsync()
        {
            var sistemaId = await GetSistemaIdAsync();

            var usuarios = await context.Usuarios.AsNoTracking()
            .Include(x => x.AspNetUser).ThenInclude(x => x.Roles)
            .Include(x => x.UsuarioEmpresas).ThenInclude(x => x.Empresa)
            .AsSplitQuery()
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
                Empresas = x.UsuarioEmpresas
                    .Where(e => e.Activo == true)
                    .Select(e => e.Empresa.Nombre)
                    .OrderBy(e => e)
                    .ToList(),
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
                .Include(x => x.AspNetUser).ThenInclude(x => x.Roles)
                .Include(x => x.UsuarioEmpresas)
                .AsSplitQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (usuario == null) return null;

            return MapDetalle(usuario, sistemaId);
        }

        public async Task<ServiceResult<UsuarioDetalleDto>> CreateAsync(UsuarioCreateDto dto, Guid usuarioCreacion)
        {
            var email = dto.Email.Trim().ToLowerInvariant();

            if (await userManager.FindByEmailAsync(email) != null)
                return ServiceResult<UsuarioDetalleDto>.Fail("Ya existe un usuario con ese correo.");

            var rolesResult = await ValidarRolesAsync(dto.Roles);
            if (!rolesResult.Success)
                return ServiceResult<UsuarioDetalleDto>.Fail(rolesResult.Message!);

            var rol = rolesResult.Data![0];
            var esAdministrador = rol.Equals("ADMINISTRADOR", StringComparison.OrdinalIgnoreCase);

            var empresasResult = await ValidarEmpresasAsync(dto.EmpresaIds, esAdministrador);
            if (!empresasResult.Success)
                return ServiceResult<UsuarioDetalleDto>.Fail(empresasResult.Message!);

            var identityUser = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var identityResult = await userManager.CreateAsync(identityUser, dto.Password);
            if (!identityResult.Succeeded)
                return ServiceResult<UsuarioDetalleDto>.Fail(GetIdentityErrors(identityResult));

            var usuarioId = Guid.NewGuid();

            try
            {
                var roleResult = await userManager.AddToRoleAsync(identityUser, rol);
                if (!roleResult.Succeeded)
                {
                    await userManager.DeleteAsync(identityUser);
                    return ServiceResult<UsuarioDetalleDto>.Fail(GetIdentityErrors(roleResult));
                }

                var usuario = new Usuario
                {
                    Id = usuarioId,
                    AspNetUserId = identityUser.Id,
                    Nombre = dto.Nombre.Trim(),
                    Apellidos = dto.Apellidos.Trim(),
                    Email = email,
                    Telefono = Normalizar(dto.Telefono),
                    Celular = Normalizar(dto.Celular),
                    Pais = Normalizar(dto.Pais),
                    CodigoPais = Normalizar(dto.CodigoPais),
                    Empresa = null,
                    DominioPrincipal = NormalizarDominio(dto.DominioPrincipal),
                    EsAdministrador = esAdministrador,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                foreach (var empresaId in empresasResult.Data!)
                {
                    usuario.UsuarioEmpresas.Add(new UsuarioEmpresa
                    {
                        UsuarioId = usuario.Id,
                        EmpresaId = empresaId,
                        Activo = true,
                        UsuarioCreacion = usuarioCreacion
                    });
                }

                context.Usuarios.Add(usuario);
                await context.SaveChangesAsync();
            }
            catch
            {
                await userManager.DeleteAsync(identityUser);
                throw;
            }

            var detalle = await GetByIdAsync(usuarioId);
            return ServiceResult<UsuarioDetalleDto>.Ok(detalle!);
        }

        public async Task<ServiceResult<UsuarioDetalleDto>> UpdateAsync(Guid id, UsuarioUpdateDto dto, Guid usuarioModificacion)
        {
            var usuario = await context.Usuarios.FirstOrDefaultAsync(x => x.Id == id);
            if (usuario == null)
                return ServiceResult<UsuarioDetalleDto>.Fail("Usuario no encontrado.");

            var identityUser = await userManager.FindByIdAsync(usuario.AspNetUserId);
            if (identityUser == null)
                return ServiceResult<UsuarioDetalleDto>.Fail("El usuario no tiene una cuenta de acceso válida.");

            var email = dto.Email.Trim().ToLowerInvariant();
            var existente = await userManager.FindByEmailAsync(email);

            if (existente != null && existente.Id != identityUser.Id)
                return ServiceResult<UsuarioDetalleDto>.Fail("Ya existe otro usuario con ese correo.");

            List<Guid>? empresaIds = null;

            if (dto.EmpresaIds != null)
            {
                var rolesActuales = await userManager.GetRolesAsync(identityUser);
                var esAdministrador = rolesActuales.Any(x =>
                    x.Equals("ADMINISTRADOR", StringComparison.OrdinalIgnoreCase));

                var empresasResult = await ValidarEmpresasAsync(dto.EmpresaIds, esAdministrador);
                if (!empresasResult.Success)
                    return ServiceResult<UsuarioDetalleDto>.Fail(empresasResult.Message!);

                empresaIds = empresasResult.Data!;
            }

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
            usuario.Empresa = null;
            usuario.DominioPrincipal = NormalizarDominio(dto.DominioPrincipal);
            usuario.FechaActualizacion = DateTime.UtcNow;

            if (empresaIds != null)
            {
                await context.Entry(usuario).Collection(x => x.UsuarioEmpresas).LoadAsync();
                SincronizarEmpresas(usuario, empresaIds, usuarioModificacion);
            }

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

            var nuevoRol = rolesResult.Data![0];

            if (!nuevoRol.Equals("ADMINISTRADOR", StringComparison.OrdinalIgnoreCase))
            {
                var tieneEmpresa = await context.UsuarioEmpresas.AsNoTracking()
                    .AnyAsync(x => x.UsuarioId == id && x.Activo == true && x.Empresa.Activo == true);

                if (!tieneEmpresa)
                    return ServiceResult<UsuarioDetalleDto>.Fail("Asigna al menos una empresa activa antes de cambiar el rol.");
            }

            var sistemaId = await GetSistemaIdAsync();

            if (!sistemaId.HasValue)
                return ServiceResult<UsuarioDetalleDto>.Fail("No se encontró configurado el sistema NEXTAG.");

            var rolesNextagDisponibles = await roleManager.Roles.AsNoTracking()
                .Where(x => x.SistemaId == sistemaId.Value && x.Name != null)
                .Select(x => x.Name!)
                .ToListAsync();

            var rolesActuales = await userManager.GetRolesAsync(identityUser);

            var rolesActualesNextag = rolesActuales
                .Where(x => rolesNextagDisponibles.Contains(x, StringComparer.OrdinalIgnoreCase))
                .ToList();

            var remover = rolesActualesNextag
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
                .Where(x => x.Clave == "NEXTAG")
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();
        }

        private async Task<ServiceResult<List<string>>> ValidarRolesAsync(List<string> roles)
        {
            var sistemaId = await GetSistemaIdAsync();

            if (!sistemaId.HasValue)
                return ServiceResult<List<string>>.Fail("No se encontró configurado el sistema NEXTAG.");

            var disponibles = await roleManager.Roles.AsNoTracking()
                .Where(x => x.SistemaId == sistemaId.Value && x.Name != null)
                .Select(x => x.Name!)
                .ToListAsync();

            var lookup = disponibles.ToDictionary(x => x.ToUpperInvariant(), x => x);
            var solicitados = (roles ?? new List<string>()).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).Distinct().ToList();

            if (solicitados.Count != 1) return ServiceResult<List<string>>.Fail("El usuario debe tener exactamente un rol de NexTag.");
            var invalidos = solicitados.Where(x => !lookup.ContainsKey(x)).ToList();

            if (invalidos.Count > 0)
                return ServiceResult<List<string>>.Fail($"Roles no válidos para NEXTAG: {string.Join(", ", invalidos)}.");

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
                EmpresaIds = usuario.UsuarioEmpresas
                    .Where(x => x.Activo == true)
                    .Select(x => x.EmpresaId)
                    .ToList(),
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
        private static void SincronizarEmpresas(Usuario usuario, List<Guid> empresaIds, Guid usuarioModificacion)
        {
            var ids = empresaIds.ToHashSet();
            var fecha = DateTime.UtcNow;

            foreach (var relacion in usuario.UsuarioEmpresas)
            {
                var debeEstarActiva = ids.Contains(relacion.EmpresaId);
                if (relacion.Activo == debeEstarActiva) continue;

                relacion.Activo = debeEstarActiva;
                relacion.FechaActualizacion = fecha;
                relacion.UsuarioModificacion = usuarioModificacion;

                if (debeEstarActiva) relacion.FechaAsignacion = fecha;
            }

            var existentes = usuario.UsuarioEmpresas.Select(x => x.EmpresaId).ToHashSet();

            foreach (var empresaId in ids.Where(x => !existentes.Contains(x)))
            {
                usuario.UsuarioEmpresas.Add(new UsuarioEmpresa
                {
                    UsuarioId = usuario.Id,
                    EmpresaId = empresaId,
                    Activo = true,
                    UsuarioCreacion = usuarioModificacion
                });
            }
        }

        private async Task<ServiceResult<List<Guid>>> ValidarEmpresasAsync(List<Guid>? empresaIds, bool esAdministradorGlobal)
        {
            var ids = (empresaIds ?? new List<Guid>()).Distinct().ToList();

            if (ids.Any(x => x == Guid.Empty))
                return ServiceResult<List<Guid>>.Fail("Se recibió un identificador de empresa inválido.");

            if (ids.Count == 0 && !esAdministradorGlobal)
                return ServiceResult<List<Guid>>.Fail("El usuario debe tener al menos una empresa asignada.");

            var cantidad = await context.Empresas.AsNoTracking()
                .CountAsync(x => ids.Contains(x.Id) && x.Activo == true);

            if (cantidad != ids.Count)
                return ServiceResult<List<Guid>>.Fail("Una o varias empresas no existen o están inactivas.");

            return ServiceResult<List<Guid>>.Ok(ids);
        }
    }
}
