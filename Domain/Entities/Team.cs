namespace Domain.Entities;

public class Team
{
    public int Id { get; set; }
    public string TeamName { get; set; }

    public int SportId { get; set; }
    public Sport Sport { get; set; }

    public ICollection<Odds> Odds { get; set; } = new List<Odds>();
}