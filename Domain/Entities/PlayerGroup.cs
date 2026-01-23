namespace Domain.Entities;

public class PlayerGroup
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public byte[] JoinedAt { get; set; } = null!;
    public int IsGroupOwner { get; set; }
    public int GroupId { get; set; }
    
    public Group Group { get; set; } = null!;
    public Player Player { get; set; } = null!;
}

