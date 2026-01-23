namespace Domain.Entities;

public class BetslipReaction
{
    public int Id { get; set; }
    public int ReactionType { get; set; }
    public int BetSlipPostId { get; set; }
    public int PlayerId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public BetSlipPost BetSlipPost { get; set; } = null!;
    public Player Player { get; set; } = null!;
}

