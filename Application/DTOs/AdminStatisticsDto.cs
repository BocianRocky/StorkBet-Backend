namespace Application.DTOs;

public class WinLossRatioDto
{
    public int TotalBets { get; set; }
    public int WonBets { get; set; }
    public int LostBets { get; set; }
    public decimal WinRatePercent { get; set; }
    public decimal LossRatePercent { get; set; }
}

public class MonthlyCouponsDto
{
    public string MonthName { get; set; } = string.Empty;
    public int BetsCount { get; set; }
    public int TotalYearCount { get; set; }
}

public class BookmakerProfitDto
{
    public decimal TotalStake { get; set; }
    public decimal TotalWinnings { get; set; }
    public decimal BookmakerProfit { get; set; }
}

public class MonthlyAverageStakeDto
{
    public int Month { get; set; }
    public decimal AverageStake { get; set; }
}

public class SportCouponsDto
{
    public int SportId { get; set; }
    public string SportName { get; set; } = string.Empty;
    public int BetSlipCount { get; set; }
}

public class SportEffectivenessDto
{
    public int SportId { get; set; }
    public string SportName { get; set; } = string.Empty;
    public int TotalBets { get; set; }
    public int WonBets { get; set; }
    public decimal EffectivenessPercent { get; set; }
}
