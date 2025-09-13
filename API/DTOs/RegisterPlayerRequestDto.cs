namespace API.DTOs;

public class RegisterPlayerRequestDto
{
    public string Name { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}   