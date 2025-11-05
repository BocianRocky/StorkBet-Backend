using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class CreateGroupRequestDto
{
    [Required]
    [MaxLength(100)]
    public string GroupName { get; set; } = string.Empty;
}
