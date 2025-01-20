using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Model.DTOs.Input;
using Model.DTOs.Output;
using Service.UserService;

namespace MobileSeriousGame.Controllers.User;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase, IUserController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    
    public async Task<ActionResult<LoginResultDTO>> Login([FromBody] LoginDTO loginDto)
    {
        var token = await _userService.AuthenticateAsync(loginDto.Email, loginDto.Password);
    
        if (token == "")
            return Unauthorized("Invalid login attempt.");

        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = jwtTokenHandler.ReadJwtToken(token);
        var expiration = jwtToken.ValidTo;
        var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? string.Empty;
        var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? string.Empty;

        var loginResponse = new LoginResultDTO
    {
        Token = token,
        Expiration = expiration,
        Email = email,
        Username = username
    };

        return Ok(loginResponse);
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResultDTO>> Register([FromBody] RegisterDTO registerDto)
    {
        var result = await _userService.RegisterAsync(registerDto);
    
        if (!result.Success)
            return BadRequest(new { errors = result.Errors });
        
        return Ok(result);
    }

    [HttpGet("users")]
    [Authorize]
    public async Task<ActionResult<List<UserResultDTO>>> GetAllUsers()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }
    [HttpPut("description")]
    [Authorize]
    public async Task<ActionResult> UpdateDescription([FromBody] DescriptionDTO updateDescriptionDto)
    {
        var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value 
                        ?? throw new UnauthorizedAccessException("No email found in token");

        var result = await _userService.UpdateDescriptionAsync(userEmail, updateDescriptionDto.Description);

        if (!result)
        {
            return BadRequest("Failed to update description.");
        }

        return Ok("Description updated successfully.");
    }
}