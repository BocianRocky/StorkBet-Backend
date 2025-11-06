using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class SendGroupMessageRequestDto
{
    [Required]
    [MaxLength(100)]
    public string MessageText { get; set; } = string.Empty;
}


