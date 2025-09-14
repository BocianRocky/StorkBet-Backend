using System;
using System.Collections.Generic;

namespace Infrastructure.Data;

public partial class Event
{
    public int Id { get; set; }

    public string? ApiId { get; set; }

    public string EventStatus { get; set; } = null!;

    public string EventName { get; set; } = null!;

    public DateTime EventDate { get; set; }

    public DateTime EventDateEnd { get; set; }

    public int SportId { get; set; }

    public virtual ICollection<Odd> Odds { get; set; } = new List<Odd>();

    public virtual Sport Sport { get; set; } = null!;
}
