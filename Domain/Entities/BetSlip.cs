namespace Domain.Entities;

public class BetSlip
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public int? Wynik { get; set; }
    public decimal? PotentialWin { get; set; }
    public int? AvailablePromotionId { get; set; }
    
    public Player Player { get; set; } = null!;
    public AvailablePromotion? AvailablePromotion { get; set; }
    public ICollection<BetSlipOdd> BetSlipOdds { get; set; } = new List<BetSlipOdd>();
    public ICollection<BetSlipPost> BetSlipPosts { get; set; } = new List<BetSlipPost>();
}

