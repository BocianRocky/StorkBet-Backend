using System;
using System.Collections.Generic;

namespace Infrastructure.Data;

public partial class Event
{
    public int Id { get; set; }

    public string EventStatus { get; set; } = null!;

    public string EventName { get; set; } = null!;

    public DateTime EventDate { get; set; }

    public DateTime EventDateEnd { get; set; }

    public virtual ICollection<EventTeam> EventTeams { get; set; } = new List<EventTeam>();

    public virtual ICollection<Odd> Odds { get; set; } = new List<Odd>();
}
