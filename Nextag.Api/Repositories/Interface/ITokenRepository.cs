using Microsoft.AspNetCore.Identity;

namespace Nextag.Api.Repositories.Interface
{
    public interface ITokenRepository
    {
        string CreateJwtToken(IdentityUser user, string usuarioId, List<string> roles);
    }
}
