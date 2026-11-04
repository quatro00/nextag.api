using System.Security.Claims;

namespace Nextag.Api.Helpers
{
    public interface IClaimsPrincipalHelper
    {
        Claim? GetUserId(ClaimsPrincipal principal);
    }
    public class ClaimsPrincipalHelper : IClaimsPrincipalHelper
    {
        public Claim? GetUserId(ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(item => item.Type.Equals("Id", StringComparison.Ordinal));
        }
    }
    public static class ExtensionMethod
    {
        public static IClaimsPrincipalHelper claimsPrincipalHelper { get; set; }
        public static string GetId(this ClaimsPrincipal User)
        {
            return User.FindFirst("Id").Value;
        }

        public static Guid GetUsuarioId(this ClaimsPrincipal User)
        {
            return Guid.Parse(User.FindFirst("UsuarioId").Value);
        }
        public static Guid GetEmpresaId(this HttpContext context)
        {
            if (context.Items.TryGetValue("EmpresaId", out var value) && value is Guid empresaId)
                return empresaId;

            throw new InvalidOperationException("No existe una empresa validada para esta solicitud.");
        }
    }
}
