namespace Domain.Entities;

public class Odds
{
    public int Id { get; set; }
    public decimal OddsValue { get; set; }
    public int? Wynik { get; set; }
    public DateTime? LastUpdate { get; set; }
    
    public int EventId { get; set; }
    public Event Event { get; set; }

    public int TeamId { get; set; }
    public Team Team { get; set; }
}