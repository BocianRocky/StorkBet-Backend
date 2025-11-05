using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class AddMemberToGroupRequestDto
{
    [Required]
    public int PlayerId { get; set; }
}
