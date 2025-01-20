namespace Model.DTOs.Output;

public class UserResultDTO
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Description { get; set; }
}