using System;
using System.Collections.Generic;

namespace Infrastructure.Data;

public partial class EventTeam
{
    public int Id { get; set; }

    public int TeamId { get; set; }

    public int EventId { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual Team Team { get; set; } = null!;
}
