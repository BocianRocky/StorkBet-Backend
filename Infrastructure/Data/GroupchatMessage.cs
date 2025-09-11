using System;
using System.Collections.Generic;

namespace Infrastructure.Data;

public partial class GroupchatMessage
{
    public int Id { get; set; }

    public int GroupId { get; set; }

    public int PlayerId { get; set; }

    public byte[] Time { get; set; } = null!;

    public string MessageText { get; set; } = null!;

    public virtual Group Group { get; set; } = null!;

    public virtual Player Player { get; set; } = null!;
}
