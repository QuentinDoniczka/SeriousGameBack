

using Microsoft.AspNetCore.Identity;
using Model.Data;

namespace Service.TokenService
{
    public interface ITokenService
    {
        Task<string> GenerateJwtToken(User user);
        
    }
}