namespace Domain.Entities;

public class Transaction
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public int Type { get; set; }
    public int PaymentMethod { get; set; }
    public int PlayerId { get; set; }
    
    public Player Player { get; set; } = null!;
}

