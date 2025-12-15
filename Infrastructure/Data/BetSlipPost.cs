using System;
using System.Collections.Generic;

namespace Infrastructure.Data;

public partial class BetSlipPost
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public int PlayerId { get; set; }

    public int BetSlipId { get; set; }

    public bool IsActive { get; set; }

    public virtual BetSlip BetSlip { get; set; } = null!;

    public virtual ICollection<BetslipReaction> BetslipReactions { get; set; } = new List<BetslipReaction>();

    public virtual Player Player { get; set; } = null!;
}
