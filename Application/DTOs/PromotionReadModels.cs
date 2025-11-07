namespace Application.DTOs;

public class PromotionReadModel
{
    public int Id { get; set; }
    public string PromotionName { get; set; } = string.Empty;
    public DateOnly DateStart { get; set; }
    public DateOnly DateEnd { get; set; }
    public string BonusType { get; set; } = string.Empty;
    public decimal BonusValue { get; set; }
    public string? PromoCode { get; set; }
    public decimal? MinDeposit { get; set; }
    public decimal? MaxDeposit { get; set; }
    public string Image { get; set; } = string.Empty;
    public string Description { get; set; }
}

public class PlayerPromotionReadModel : PromotionReadModel
{
    public int AvailablePromotionId { get; set; }
    public string Availability { get; set; } = string.Empty;
}


