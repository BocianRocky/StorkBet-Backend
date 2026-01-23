namespace Domain.Entities;

public class Group
{
    public int Id { get; set; }
    public string GroupName { get; set; } = null!;
    
    public ICollection<GroupchatMessage> GroupchatMessages { get; set; } = new List<GroupchatMessage>();
    public ICollection<PlayerGroup> PlayerGroups { get; set; } = new List<PlayerGroup>();
}

