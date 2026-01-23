namespace Domain.Entities;

public class BetSlipOdd
{
    public int Id { get; set; }
    public int BetSlipId { get; set; }
    public int? Wynik { get; set; }
    public decimal ConstOdd { get; set; }
    public int OddsId { get; set; }
    
    public BetSlip BetSlip { get; set; } = null!;
    public Odds Odds { get; set; } = null!;
}

