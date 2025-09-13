namespace API.DTOs;

public class LoginResponseDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExp { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public decimal AccountBalance { get; set; }
}

