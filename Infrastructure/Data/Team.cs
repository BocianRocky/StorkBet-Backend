using System;
using System.Collections.Generic;

namespace Infrastructure.Data;

public partial class Team
{
    public int Id { get; set; }

    public string TeamName { get; set; } = null!;

    public int SportId { get; set; }

    public virtual ICollection<EventTeam> EventTeams { get; set; } = new List<EventTeam>();

    public virtual ICollection<Odd> Odds { get; set; } = new List<Odd>();

    public virtual Sport Sport { get; set; } = null!;
}
