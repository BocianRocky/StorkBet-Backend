using System;
using System.Collections.Generic;

namespace Infrastructure.Data;

public partial class Player
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Salt { get; set; } = null!;

    public decimal AccountBalance { get; set; }

    public string RefreshToken { get; set; } = null!;

    public DateTime RefreshTokenExp { get; set; }

    public virtual ICollection<AvailablePromotion> AvailablePromotions { get; set; } = new List<AvailablePromotion>();

    public virtual ICollection<BetSlip> BetSlips { get; set; } = new List<BetSlip>();

    public virtual ICollection<GroupchatMessage> GroupchatMessages { get; set; } = new List<GroupchatMessage>();

    public virtual ICollection<PlayerGroup> PlayerGroups { get; set; } = new List<PlayerGroup>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
