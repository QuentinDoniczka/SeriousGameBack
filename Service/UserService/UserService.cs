using Microsoft.AspNetCore.Identity;
using Model.Data;
using Model.DTOs.Input;
using Model.DTOs.Output;
using Service.TokenService;

namespace Service.UserService;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    public UserService(UserManager<User> userManager, ITokenService tokenService)
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
        var result = new RegisterResultDTO();
        
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            result.Success = false;
            result.Errors.Add("Email already exists");
            return result;
        }
        
        var user = new User
        {
            UserName = registerDto.Username,
            Email = registerDto.Email
        };
        
        var createResult = await _userManager.CreateAsync(user, registerDto.Password);

        if (createResult.Succeeded)
        {
            result.Success = true;
        }
        else
        {
            result.Success = false;
            result.Errors.AddRange(createResult.Errors.Select(e => e.Description));
        }

        return result;
    }

    public async Task<List<UserResultDTO>> GetAllAsync()
    {
        var users = _userManager.Users.ToList();
        var userDTOs = users.Select(user => new UserResultDTO
        {
            Email = user.Email ?? string.Empty,
            Username = user.UserName ?? string.Empty,
            Description = user.Description
        }).ToList();
    
        return userDTOs;
    }

    public async Task<bool> UpdateDescriptionAsync(string email, string? description)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return false;
        }

        user.Description = description;
        var result = await _userManager.UpdateAsync(user);

        return result.Succeeded;
    }
}