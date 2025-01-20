using Microsoft.AspNetCore.Mvc;
using Model.DTOs.Input;
using Model.DTOs.Output;

namespace MobileSeriousGame.Controllers.User;

public interface IUserController
{
    Task<ActionResult<LoginResultDTO>> Login(LoginDTO loginDto);
    Task<ActionResult<RegisterResultDTO>> Register(RegisterDTO registerDto);
    Task<ActionResult<List<UserResultDTO>>> GetAllUsers();
    Task<ActionResult> UpdateDescription(DescriptionDTO updateDescriptionDto);
}