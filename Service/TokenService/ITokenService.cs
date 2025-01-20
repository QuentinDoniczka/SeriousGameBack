

using Microsoft.AspNetCore.Identity;

namespace Service.TokenService
{
    public interface ITokenService
    {
        Task<string> GenerateJwtToken(IdentityUser user);
        
    }
}