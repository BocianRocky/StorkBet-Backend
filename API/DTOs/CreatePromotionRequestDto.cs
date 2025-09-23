using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class CreatePromotionRequestDto
{
    [Required]
    [MaxLength(20)]
    public string PromotionName { get; set; } = string.Empty;

    [Required]
    public DateOnly DateStart { get; set; }

    [Required]
    public DateOnly DateEnd { get; set; }

    [Required]
    [MaxLength(50)]
    public string BonusType { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal BonusValue { get; set; }

    [MaxLength(50)]
    public string? PromoCode { get; set; }

    public decimal? MinDeposit { get; set; }

    public decimal? MaxDeposit { get; set; }
    
    [MaxLength(70)]
    public string Image { get; set; }
}


