using Microsoft.AspNetCore.Identity;

namespace patchikatcha_backend.Repositories
{
    public interface IToken
    {
        string CreateJTWToken(IdentityUser user, List<string> roles);
    }
}
