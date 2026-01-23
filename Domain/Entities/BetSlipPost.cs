namespace Domain.Entities;

public class BetSlipPost
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public int PlayerId { get; set; }
    public int BetSlipId { get; set; }
    public bool IsActive { get; set; }
    
    public BetSlip BetSlip { get; set; } = null!;
    public Player Player { get; set; } = null!;
    public ICollection<BetslipReaction> BetslipReactions { get; set; } = new List<BetslipReaction>();
}

