namespace Model.DTOs.Output;

public class LoginResultDTO
{
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Token { get; set; } = null!;
    public DateTime Expiration { get; set; }
}