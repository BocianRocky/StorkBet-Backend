namespace API.DTOs;

public class BetSlipListItemDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public int? Wynik { get; set; }
    public decimal TotalOdds { get; set; }
    public decimal PotentialWin { get; set; }
    public int OddsCount { get; set; }
}
