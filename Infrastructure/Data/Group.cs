using System;
using System.Collections.Generic;

namespace Infrastructure.Data;

public partial class Group
{
    public int Id { get; set; }

    public string GroupName { get; set; } = null!;

    public virtual ICollection<GroupchatMessage> GroupchatMessages { get; set; } = new List<GroupchatMessage>();

    public virtual ICollection<PlayerGroup> PlayerGroups { get; set; } = new List<PlayerGroup>();
}
