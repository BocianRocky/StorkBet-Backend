namespace API.DTOs;

public class BetSlipDetailsDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public int? Wynik { get; set; }
    public decimal TotalOdds { get; set; }
    public decimal PotentialWin { get; set; }
    public List<BetSlipOddDetailsDto> BetSlipOdds { get; set; } = new();
}

public class BetSlipOddDetailsDto
{
    public int Id { get; set; }
    public decimal ConstOdd { get; set; }
    public int? Wynik { get; set; }
    public EventDetailsDto Event { get; set; } = null!;
    public TeamDetailsDto Team { get; set; } = null!;
}

public class EventDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public class TeamDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
