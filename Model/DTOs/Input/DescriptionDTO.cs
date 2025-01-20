using Swashbuckle.AspNetCore.Annotations;

namespace Model.DTOs.Input;

/// <summary>
/// Description data transfer object
/// </summary>
[SwaggerSchema(
    Title = "Description Model",
    Description = "Model for updating user description (optional)"
)]
public class DescriptionDTO
{
    /// <summary>
    /// User's optional personal description or bio
    /// </summary>
    /// <example>Hi! I'm a software developer passionate about C# and .NET</example>
    public string? Description { get; set; }
}