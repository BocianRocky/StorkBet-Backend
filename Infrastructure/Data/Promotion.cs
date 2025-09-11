using System;
using System.Collections.Generic;

namespace Infrastructure.Data;

public partial class Promotion
{
    public int Id { get; set; }

    public string PromotionName { get; set; } = null!;

    public DateOnly DateStart { get; set; }

    public DateOnly DateEnd { get; set; }

    public virtual ICollection<AvailablePromotion> AvailablePromotions { get; set; } = new List<AvailablePromotion>();
}
