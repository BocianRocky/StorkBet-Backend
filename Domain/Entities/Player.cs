namespace Domain.Entities;

public class Player
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Salt { get; set; }
    public decimal AccountBalance { get; set; }
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExp { get; set; }
}
