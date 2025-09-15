namespace Application.DTOs;

public class EventWithOddsDto
{
    public int EventId { get; set; }
    public DateTime EventDate { get; set; }
    public List<OddDto> Odds { get; set; } = new List<OddDto>();
}

public class OddDto
{
    public string TeamName { get; set; } = string.Empty;
    public decimal OddsValue { get; set; }
}
