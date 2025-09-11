using System;
using System.Collections.Generic;

namespace Infrastructure.Data;

public partial class Transaction
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public int Type { get; set; }

    public int PaymentMethod { get; set; }

    public int PlayerId { get; set; }

    public virtual Player Player { get; set; } = null!;
}
