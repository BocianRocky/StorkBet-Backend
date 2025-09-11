using System;
using System.Collections.Generic;

namespace Infrastructure.Data;

public partial class PlayerGroup
{
    public int Id { get; set; }

    public int PlayerId { get; set; }

    public byte[] JoinedAt { get; set; } = null!;

    public int IsGroupOwner { get; set; }

    public int GroupId { get; set; }

    public virtual Group Group { get; set; } = null!;

    public virtual Player Player { get; set; } = null!;
}
