namespace Application.DTOs;

public class BetSlipZoneItemDto
{
    public int PostId { get; set; }
    public int BetSlipId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string PlayerLastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    
    
    public decimal Amount { get; set; }
    public decimal TotalOdds { get; set; }
    public decimal PotentialWin { get; set; }
    public int OddsCount { get; set; }
    public int? Wynik { get; set; }
    
    
    public int FireCount { get; set; }
    public int ColdCount { get; set; }
    public int SafeCount { get; set; }
    public int CrazyCount { get; set; }
    public int? UserReaction { get; set; } 
}

public class BetSlipZoneDetailsDto
{
    public int PostId { get; set; }
    public int BetSlipId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string PlayerLastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    
    
    public BetSlipDetailsDto BetSlip { get; set; } = null!;
    
    
    public int FireCount { get; set; }
    public int ColdCount { get; set; }
    public int SafeCount { get; set; }
    public int CrazyCount { get; set; }
    public int? UserReaction { get; set; }
}

public class CreateBetSlipPostRequestDto
{
    public int BetSlipId { get; set; }
}

public class SetBetSlipReactionRequestDto
{
    public int ReactionType { get; set; } // 1 = Fire, 2 = Cold, 3 = Safe, 4 = Crazy
}

public class BetSlipReactionSummaryDto
{
    public int FireCount { get; set; }
    public int ColdCount { get; set; }
    public int SafeCount { get; set; }
    public int CrazyCount { get; set; }
    public int? UserReaction { get; set; }
}


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
    public string Group { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public class TeamDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}


