namespace Domain.Entities;

public class GroupchatMessage
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int PlayerId { get; set; }
    public byte[] Time { get; set; } = null!;
    public string MessageText { get; set; } = null!;
    
    public Group Group { get; set; } = null!;
    public Player Player { get; set; } = null!;
}

