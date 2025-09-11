using System;
using System.Collections.Generic;

namespace Infrastructure.Data;

public partial class BetSlipOdd
{
    public int Id { get; set; }

    public int BetSlipId { get; set; }

    public int OddsOddsId { get; set; }

    public int? Wynik { get; set; }

    public virtual BetSlip BetSlip { get; set; } = null!;

    public virtual Odd OddsOdds { get; set; } = null!;
}
