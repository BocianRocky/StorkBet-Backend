namespace Domain.Entities;

public class RegisterPlayer
{
    public string Name { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    
    public string Password { get; set; }
    public string Salt { get; set; }
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExp { get; set; }
    
}