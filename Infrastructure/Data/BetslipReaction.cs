using System;
using System.Collections.Generic;

namespace Infrastructure.Data;

public partial class BetslipReaction
{
    public int Id { get; set; }

    public int ReactionType { get; set; }

    public int BetSlipPostId { get; set; }

    public int PlayerId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual BetSlipPost BetSlipPost { get; set; } = null!;

    public virtual Player Player { get; set; } = null!;
}
