namespace Domain.Entities;

public class Event
{
    public int Id { get; set; }
    public string ApiId { get; set; }
    public string EventStatus { get; set; }
    public string EventName { get; set; }
    public DateTime CommenceTime { get; set; }
    public DateTime EndTime { get; set; }
    
    
    public int SportId { get; set; }
    public Sport Sport { get; set; }

    public ICollection<Odds> Odds { get; set; } = new List<Odds>();
}