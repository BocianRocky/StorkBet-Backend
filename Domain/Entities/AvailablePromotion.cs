namespace Domain.Entities;

public class AvailablePromotion
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public int PromotionId { get; set; }
    public string Availability { get; set; } = null!;
    
    public Player Player { get; set; } = null!;
    public Promotion Promotion { get; set; } = null!;
    public ICollection<BetSlip> BetSlips { get; set; } = new List<BetSlip>();
}

