using Microsoft.AspNetCore.Identity;

namespace SIGED.api.Repositories.Interface
{
    public interface ITokenRepository
    {
        string CreateJwtToken(IdentityUser user, string usuarioId, List<string> roles);
    }
}
