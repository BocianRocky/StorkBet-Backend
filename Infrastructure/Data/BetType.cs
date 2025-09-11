using System;
using System.Collections.Generic;

namespace Infrastructure.Data;

public partial class BetType
{
    public int Id { get; set; }

    public string BetTypeName { get; set; } = null!;

    public virtual ICollection<Odd> Odds { get; set; } = new List<Odd>();
}
