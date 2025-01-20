using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace Model.DTOs.Input;

/// <summary>
/// Register data transfer object
/// </summary>
[SwaggerSchema(
    Title = "Register Model",
    Description = "Model for user registration credentials"
)]
public class RegisterDTO
{
    /// <summary>
    /// User's username
    /// </summary>
    /// <example>test_user</example>
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = null!;

    /// <summary>
    /// User's email address
    /// </summary>
    /// <example>user@example.com</example>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = null!;

    /// <summary>
    /// User's password
    /// </summary>
    /// <example>Test123!Test123!</example>
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = null!;
}