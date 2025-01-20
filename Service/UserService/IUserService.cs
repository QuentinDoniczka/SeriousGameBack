using Model.DTOs.Input;
using Model.DTOs.Output;

namespace Service.UserService;

public interface IUserService
{
    Task<string> AuthenticateAsync(string email, string password);
    Task<RegisterResultDTO> RegisterAsync(RegisterDTO registerDto);
    Task<List<UserResultDTO>> GetAllAsync();
    Task<bool> UpdateDescriptionAsync(string email, string? description);
}