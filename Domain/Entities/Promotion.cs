namespace Domain.Entities;

public class Promotion
{
    public int Id { get; set; }
    public string PromotionName { get; set; } = null!;
    public DateOnly DateStart { get; set; }
    public DateOnly DateEnd { get; set; }
    public string BonusType { get; set; } = null!;
    public decimal BonusValue { get; set; }
    public string? PromoCode { get; set; }
    public decimal? MinDeposit { get; set; }
    public decimal? MaxDeposit { get; set; }
    public string Image { get; set; } = null!;
    public string Description { get; set; } = null!;
    
    public ICollection<AvailablePromotion> AvailablePromotions { get; set; } = new List<AvailablePromotion>();
}

