namespace Application.DTOs;

public class PopularEventDto
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public int SportId { get; set; }
    public string SportKey { get; set; } = string.Empty;
    public string SportTitle { get; set; } = string.Empty;
    public string SportGroup { get; set; } = string.Empty;
    public List<OddDto> Odds { get; set; } = new List<OddDto>();
    public int BetCount { get; set; }
}

