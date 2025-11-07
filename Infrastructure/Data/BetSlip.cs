using System;
using System.Collections.Generic;

namespace Infrastructure.Data;

public partial class BetSlip
{
    public int Id { get; set; }

    public int PlayerId { get; set; }

    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    public int? Wynik { get; set; }

    public decimal? PotentialWin { get; set; }

    public int? AvailablePromotionId { get; set; }

    public virtual AvailablePromotion? AvailablePromotion { get; set; }

    public virtual ICollection<BetSlipOdd> BetSlipOdds { get; set; } = new List<BetSlipOdd>();

    public virtual Player Player { get; set; } = null!;
}
