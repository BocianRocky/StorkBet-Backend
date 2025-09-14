using System;
using System.Collections.Generic;

namespace Infrastructure.Data;

public partial class Odd
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public decimal OddsValue { get; set; }

    public int? Wynik { get; set; }

    public DateTime? LastUpdate { get; set; }

    public int TeamId { get; set; }

    public virtual ICollection<BetSlipOdd> BetSlipOdds { get; set; } = new List<BetSlipOdd>();

    public virtual Event Event { get; set; } = null!;

    public virtual Team Team { get; set; } = null!;
}
