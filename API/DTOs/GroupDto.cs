namespace API.DTOs;

public class GroupDto
{
    public int Id { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public List<GroupMemberDto> Members { get; set; } = new();
    public int MessageCount { get; set; }
}

public class GroupMemberDto
{
    public int PlayerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
}
