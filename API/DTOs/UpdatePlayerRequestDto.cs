using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class UpdatePlayerRequestDto
{
    [Required]
    public int PlayerId { get; set; }
    
    public string? Name { get; set; }
    
    public string? LastName { get; set; }
    
    public string? Email { get; set; }
    
    public decimal? AccountBalance { get; set; }
    
    public int? Role { get; set; }
}
