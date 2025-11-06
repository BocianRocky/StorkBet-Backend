namespace API.DTOs;

public class GroupMessageDto
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string PlayerLastName { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
}


