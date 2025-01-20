using Microsoft.AspNetCore.Identity;
using Model.DTOs.Input;
using Model.DTOs.Output;
using Service.TokenService;

namespace Service.UserService;

public class UserService : IUserService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITokenService _tokenService;
    public UserService(UserManager<IdentityUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }
    public async Task<string> AuthenticateAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user != null && await _userManager.CheckPasswordAsync(user, password))
        {
            return await _tokenService.GenerateJwtToken(user);
        }

        return "";
    }

    public async Task<RegisterResultDTO> RegisterAsync(RegisterDTO registerDto)
    {
        // Implement registration logic
        throw new NotImplementedException();
    }

    public async Task<List<LoginResultDTO>> GetAllAsync()
    {
        // Implement get all users logic
        throw new NotImplementedException();
    }
}