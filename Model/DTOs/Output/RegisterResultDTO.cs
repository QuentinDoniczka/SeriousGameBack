namespace Model.DTOs.Output;

public class RegisterResultDTO
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}