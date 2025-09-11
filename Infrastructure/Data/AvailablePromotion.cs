using System;
using System.Collections.Generic;

namespace Infrastructure.Data;

public partial class AvailablePromotion
{
    public int Id { get; set; }

    public int PlayerId { get; set; }

    public int PromotionId { get; set; }

    public string Availability { get; set; } = null!;

    public virtual Player Player { get; set; } = null!;

    public virtual Promotion Promotion { get; set; } = null!;
}
