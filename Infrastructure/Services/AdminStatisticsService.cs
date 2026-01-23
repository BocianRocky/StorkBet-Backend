using System.Text.Json;
using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AdminStatisticsService : IAdminStatisticsService
{
    private readonly AppDbContext _context;

    public AdminStatisticsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<WinLossRatioDto> GetWinLossRatioAsync()
    {
        var sql = @"
            SELECT 
                COUNT(*) AS TotalBets,
                COUNT(CASE WHEN Wynik = 1 THEN 1 END) AS WonBets,
                COUNT(CASE WHEN Wynik = 0 THEN 1 END) AS LostBets,
                CAST(ROUND(
                    CAST(COUNT(CASE WHEN Wynik = 1 THEN 1 END) AS float) 
                    / NULLIF(COUNT(*), 0) * 100, 
                    2
                ) AS decimal(18,2)) AS WinRatePercent,
                CAST(ROUND(
                    CAST(COUNT(CASE WHEN Wynik = 0 THEN 1 END) AS float) 
                    / NULLIF(COUNT(*), 0) * 100, 
                    2
                ) AS decimal(18,2)) AS LossRatePercent
            FROM BetSlip
            WHERE Wynik IS NOT NULL";

        var result = await _context.Database
            .SqlQueryRaw<WinLossRatioDto>(sql)
            .FirstOrDefaultAsync();

        return result ?? new WinLossRatioDto();
    }

    public async Task<IEnumerable<MonthlyCouponsDto>> GetMonthlyCouponsAsync()
    {
        var sql = @"
    SELECT 
        DATENAME(MONTH, Date) AS MonthName,
        COUNT(*) AS BetsCount,
        SUM(COUNT(*)) OVER() AS TotalYearCount
    FROM BetSlip
    WHERE YEAR(Date) = YEAR(GETDATE())
    GROUP BY DATENAME(MONTH, Date),MONTH(Date)
    ORDER BY MONTH(Date)";

        var result = await _context.Database
            .SqlQueryRaw<MonthlyCouponsDto>(sql)
            .ToListAsync();

        return result;
    }

    public async Task<BookmakerProfitDto> GetBookmakerProfitAsync()
    {
        var sql = @"
            SELECT 
                SUM(Amount) AS TotalStake,
                SUM(CASE WHEN Wynik = 1 THEN PotentialWin ELSE 0 END) AS TotalWinnings,
                SUM(Amount) - SUM(CASE WHEN Wynik = 1 THEN PotentialWin ELSE 0 END) AS BookmakerProfit
            FROM BetSlip
            WHERE Wynik IS NOT NULL";

        var result = await _context.Database
            .SqlQueryRaw<BookmakerProfitDto>(sql)
            .FirstOrDefaultAsync();

        return result ?? new BookmakerProfitDto();
    }

    public async Task<IEnumerable<MonthlyAverageStakeDto>> GetMonthlyAverageStakeAsync()
    {
        var sql = @"
            SELECT 
                MONTH(Date) AS Month,
                CAST(AVG(Amount) AS decimal(18,2)) AS AverageStake
            FROM BetSlip
            WHERE YEAR(Date) = YEAR(GETDATE())
            GROUP BY MONTH(Date)
            ORDER BY Month";

        var result = await _context.Database
            .SqlQueryRaw<MonthlyAverageStakeDto>(sql)
            .ToListAsync();

        return result;
    }

    public async Task<IEnumerable<SportCouponsDto>> GetSportCouponsAsync()
    {
        var sql = @"
            WITH BetSlipSport AS (
                SELECT DISTINCT
                    bs.Id AS BetSlipId,
                    s.Id AS SportId,
                    s.Title AS SportName
                FROM BetSlip bs
                JOIN BetSlipOdds bso ON bso.BetSlipId = bs.Id
                JOIN Odds o ON o.Id = bso.Odds_Id
                JOIN Event e ON e.Id = o.EventId
                JOIN Sport s ON s.Id = e.Sport_Id
            )
            SELECT 
                SportId,
                SportName,
                COUNT(BetSlipId) AS BetSlipCount
            FROM BetSlipSport
            GROUP BY SportId, SportName
            ORDER BY BetSlipCount DESC";

        var result = await _context.Database
            .SqlQueryRaw<SportCouponsDto>(sql)
            .ToListAsync();

        return result;
    }

    public async Task<IEnumerable<SportEffectivenessDto>> GetSportEffectivenessAsync()
    {
        var sql = @"
            SELECT 
                s.Id AS SportId,
                s.Title AS SportName,
                COUNT(*) AS TotalBets,
                COUNT(CASE WHEN bso.Wynik = 1 THEN 1 END) AS WonBets,
                CAST(ROUND(
                    CAST(COUNT(CASE WHEN bso.Wynik = 1 THEN 1 END) AS float)
                    / NULLIF(COUNT(*), 0) * 100, 2
                ) AS decimal(18,2)) AS EffectivenessPercent
            FROM BetSlipOdds bso
            JOIN Odds o ON o.Id = bso.Odds_Id
            JOIN Event e ON e.Id = o.EventId
            JOIN Sport s ON s.Id = e.Sport_Id
            GROUP BY s.Id, s.Title
            ORDER BY EffectivenessPercent DESC";

        var result = await _context.Database
            .SqlQueryRaw<SportEffectivenessDto>(sql)
            .ToListAsync();

        return result;
    }

    public async Task<IEnumerable<PlayerProfitDto>> GetPlayersProfitAsync()
    {
        var sql = @"
SELECT
    p.Id AS PlayerId,
    p.Name,
    p.LastName,
    p.AccountBalance,
    ROUND(
        ISNULL(SUM(CASE WHEN bs.wynik = 1 THEN bs.PotentialWin ELSE 0 END), 0)
        - ISNULL(SUM(bs.Amount), 0),
        2
    ) AS Profit
FROM Player p
LEFT JOIN BetSlip bs ON bs.PlayerId = p.Id
WHERE Role = 2
GROUP BY p.Id, p.Name, p.LastName, p.AccountBalance
ORDER BY Profit DESC";

        var result = await _context.Database
            .SqlQueryRaw<PlayerProfitDto>(sql)
            .ToListAsync();

        return result;
    }

    public async Task<PlayerDetailsDto?> GetPlayerDetailsAsync(int playerId)
    {
        var sql = @"
SELECT
    p.Id AS PlayerId,
    p.Name,
    p.LastName,
    p.Email,
    p.AccountBalance,

    COUNT(DISTINCT bs.Id) AS BetsCount,
    COUNT(CASE WHEN bs.wynik = 1 THEN 1 END) AS WonBets,
    COUNT(CASE WHEN bs.wynik = 0 THEN 1 END) AS LostBets,

    CAST(
        ROUND(
            CAST(COUNT(CASE WHEN bs.wynik = 1 THEN 1 END) AS DECIMAL(18,2)) /
            NULLIF(COUNT(DISTINCT bs.Id), 0) * 100, 2
        ) AS DECIMAL(18,2)
    ) AS EffectivenessPercent,

    CAST(ISNULL(SUM(bs.Amount), 0) AS DECIMAL(18,2)) AS TotalStake,
    CAST(ISNULL(SUM(CASE WHEN bs.wynik = 1 THEN bs.PotentialWin ELSE 0 END), 0) AS DECIMAL(18,2)) AS TotalWinnings,

    CAST(
        ROUND(
            ISNULL(SUM(CASE WHEN bs.wynik = 1 THEN bs.PotentialWin ELSE 0 END), 0)
            - ISNULL(SUM(bs.Amount), 0),
            2
        ) AS DECIMAL(18,2)
    ) AS Profit,

    MAX(bs.Date) AS LastBetDate,

    COUNT(DISTINCT t.Id) AS TransactionsCount,
    CAST(ISNULL(SUM(CASE WHEN t.Type = 1 THEN t.Amount END), 0) AS DECIMAL(18,2)) AS TotalDeposits,
    CAST(ISNULL(SUM(CASE WHEN t.Type = 2 THEN t.Amount END), 0) AS DECIMAL(18,2)) AS TotalWithdrawals

FROM Player p
LEFT JOIN BetSlip bs ON bs.PlayerId = p.Id
LEFT JOIN [Transaction] t ON t.PlayerId = p.Id
WHERE p.Id = @playerId
GROUP BY
    p.Id, p.Name, p.LastName, p.Email, p.AccountBalance";

        var param = new Microsoft.Data.SqlClient.SqlParameter("@playerId", playerId);

        var result = await _context.Database
            .SqlQueryRaw<PlayerDetailsDto>(sql, param)
            .FirstOrDefaultAsync();

        return result;
    }
    
    
    public async Task<List<UncompletedEventsDto>> GetUncompletedEventsAsync()
    {
        try
        {
            var rows = await _context.Events
                .Where(e => e.IsCompleted == null)
                .OrderBy(e => e.EventDate)
                .SelectMany(e => e.Odds, (e, o) => new
                {
                    EventId = e.Id,
                    e.EventName,
                    e.EventDate,
                    TeamId = o.Team.Id,
                    TeamName = o.Team.TeamName,
                    o.OddsValue
                })
                .ToListAsync();
            
            var events = rows
                .GroupBy(r => new { r.EventId, r.EventName, r.EventDate })
                .Select(g => new UncompletedEventsDto
                {
                    EventId = g.Key.EventId,
                    EventName = g.Key.EventName,
                    EventDate = g.Key.EventDate,
                    Odds = g.Select(x => new UncompletedOddsDto
                    {
                        TeamId = x.TeamId,
                        TeamName = x.TeamName,
                        OddsValue = x.OddsValue
                    }).ToList()
                })
                .ToList();

            return events;
        }
        catch (Exception ex)
        {
            
            
            throw new Exception($"Błąd podczas pobierania niezakończonych wydarzeń: {ex.Message}");
        }
    }

    public async Task UpdateEventResultAsync(int eventId, int team1Id, int team2Id, int team1Score, int team2Score)
{
    // Pobranie wydarzenia wraz z kursami i drużynami
    var eventEntity = await _context.Events
        .Include(e => e.Odds)
            .ThenInclude(o => o.Team)
        .Include(e => e.Odds)
            .ThenInclude(o => o.BetSlipOdds)
                .ThenInclude(bso => bso.BetSlip)
        .FirstOrDefaultAsync(e => e.Id == eventId);

    if (eventEntity == null)
        throw new Exception($"Nie znaleziono wydarzenia o Id {eventId}.");

    // Pobranie kursów dla drużyn
    var team1Odds = eventEntity.Odds.FirstOrDefault(o => o.TeamId == team1Id);
    var team2Odds = eventEntity.Odds.FirstOrDefault(o => o.TeamId == team2Id);
    var drawOdds = eventEntity.Odds.FirstOrDefault(o => o.Team != null && o.Team.TeamName == "Draw");

    if (team1Odds == null || team2Odds == null)
        throw new Exception("Nie znaleziono kursów dla jednej lub obu drużyn.");

    // Aktualizacja wyników drużyn i remisu
    team1Odds.Wynik = team1Score;
    team2Odds.Wynik = team2Score;

    if (drawOdds != null)
        drawOdds.Wynik = (team1Score == team2Score) ? 1 : 0;

    eventEntity.IsCompleted = true;

    // Określenie zwycięzcy
    int winningTeamId = team1Score > team2Score ? team1Id :
                        team2Score > team1Score ? team2Id : -1;

    // Pobranie wszystkich powiązanych BetSlipOdds (bez null)
    var relatedBetSlipOdds = eventEntity.Odds
        .SelectMany(o => o.BetSlipOdds ?? Enumerable.Empty<BetSlipOdd>())
        .Where(bso => bso != null && bso.Odds != null)
        .ToList();

    // Aktualizacja wyników pojedynczych zakładów
    foreach (var betSlipOdd in relatedBetSlipOdds)
    {
        if (winningTeamId == -1)
        {
            // remis
            betSlipOdd.Wynik = betSlipOdd.Odds.Team.TeamName == "Draw" ? 1 : 0;
        }
        else
        {
            // zwycięzca drużyny
            betSlipOdd.Wynik = betSlipOdd.Odds.TeamId == winningTeamId ? 1 : 0;
        }
    }

    // Rozliczenie kuponów dopiero po zaktualizowaniu wszystkich BetSlipOdds
    var affectedBetSlips = relatedBetSlipOdds
        .Select(bso => bso.BetSlip)
        .Where(bs => bs != null)
        .Distinct()
        .ToList();

    foreach (var betSlip in affectedBetSlips!)
    {
        // Pobranie wszystkich zakładów na kuponie z pełnymi eventami
        var allBso = await _context.BetSlipOdds
            .Include(bso => bso.Odds)
                .ThenInclude(o => o.Event)
            .Where(bso => bso.BetSlipId == betSlip.Id)
            .ToListAsync();

        bool allEventsCompleted = allBso.All(bso => bso.Odds?.Event?.IsCompleted == true);

        if (allEventsCompleted)
        {
            // Kupon wygrany tylko jeśli wszystkie typy wygrane
            bool allOddsWon = allBso.All(bso => bso.Wynik == 1);
            betSlip.Wynik = allOddsWon ? 1 : 0;
            
            // Jeśli kupon wygrany, przypisz wygraną do konta użytkownika
            if (allOddsWon && betSlip.Wynik == 1)
            {
                var player = await _context.Players.FindAsync(betSlip.PlayerId);
                if (player != null && betSlip.PotentialWin.HasValue)
                {
                    player.AccountBalance += betSlip.PotentialWin.Value;
                }
            }
        }
        else
        {
            // Pozostaw wynik null jeśli nie wszystkie mecze zakończone
            betSlip.Wynik = null;
        }
    }

    await _context.SaveChangesAsync();
}

public async Task<IEnumerable<UserRankingDto>> GetUserRankingAsync(int topCount = 30)
{
    // Pobranie danych dla wszystkich graczy
    var sql = @"
SELECT
    p.Id AS PlayerId,
    p.Name,
    p.LastName,
    CAST(ISNULL(SUM(bs.Amount), 0) AS DECIMAL(18,2)) AS TotalStake,
    CAST(ISNULL(SUM(CASE WHEN bs.wynik = 1 THEN bs.PotentialWin ELSE 0 END), 0) AS DECIMAL(18,2)) AS TotalWinnings,
    CAST(
        ROUND(
            ISNULL(SUM(CASE WHEN bs.wynik = 1 THEN bs.PotentialWin ELSE 0 END), 0)
            - ISNULL(SUM(bs.Amount), 0),
            2
        ) AS DECIMAL(18,2)
    ) AS Profit,
    CAST(
        CASE 
            WHEN ISNULL(SUM(bs.Amount), 0) > 0 
            THEN ROUND(
                (ISNULL(SUM(CASE WHEN bs.wynik = 1 THEN bs.PotentialWin ELSE 0 END), 0)
                - ISNULL(SUM(bs.Amount), 0)) / ISNULL(SUM(bs.Amount), 0) * 100,
                2
            )
            ELSE 0
        END AS DECIMAL(18,2)
    ) AS Yield,
    CAST(
        ROUND(
            CAST(COUNT(CASE WHEN bs.wynik = 1 THEN 1 END) AS DECIMAL(18,2)) /
            NULLIF(COUNT(DISTINCT bs.Id), 0) * 100, 2
        ) AS DECIMAL(18,2)
    ) AS WinRate,
    COUNT(DISTINCT bs.Id) AS BetsCount
FROM Player p
LEFT JOIN BetSlip bs ON bs.PlayerId = p.Id
WHERE p.Role = 2
GROUP BY p.Id, p.Name, p.LastName
HAVING COUNT(DISTINCT bs.Id) > 0";

    var playerStats = await _context.Database
        .SqlQueryRaw<PlayerRankingStats>(sql)
        .ToListAsync();

    if (!playerStats.Any())
        return new List<UserRankingDto>();

    // Obliczenie min i max dla normalizacji
    var profits = playerStats.Select(p => p.Profit).ToList();
    var yields = playerStats.Select(p => p.Yield).ToList();
    var winRates = playerStats.Select(p => p.WinRate).ToList();
    var betsCounts = playerStats.Select(p => (decimal)p.BetsCount).ToList();

    var minProfit = profits.Min();
    var maxProfit = profits.Max();
    var minYield = yields.Min();
    var maxYield = yields.Max();
    var minWinRate = winRates.Min();
    var maxWinRate = winRates.Max();
    var minBetsCount = betsCounts.Min();
    var maxBetsCount = betsCounts.Max();

    // Funkcja normalizacji min-max
    decimal Normalize(decimal value, decimal min, decimal max)
    {
        if (max == min) return 0;
        return (value - min) / (max - min);
    }

    // Obliczenie score dla każdego gracza
    var rankings = playerStats.Select(stat =>
    {
        var profitNormalized = Normalize(stat.Profit, minProfit, maxProfit);
        var yieldNormalized = Normalize(stat.Yield, minYield, maxYield);
        var winRateNormalized = Normalize(stat.WinRate, minWinRate, maxWinRate);
        var betsCountNormalized = Normalize((decimal)stat.BetsCount, minBetsCount, maxBetsCount);

        var score = (profitNormalized * 0.4m) +
                   (yieldNormalized * 0.3m) +
                   (winRateNormalized * 0.15m) +
                   (betsCountNormalized * 0.15m);

        return new UserRankingDto
        {
            Id = stat.PlayerId,
            Name = stat.Name,
            LastName = stat.LastName,
            Score = Math.Round(score, 4)
        };
    })
    .OrderByDescending(r => r.Score)
    .Take(topCount)
    .ToList();

    return rankings;
}

private class PlayerRankingStats
{
    public int PlayerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public decimal TotalStake { get; set; }
    public decimal TotalWinnings { get; set; }
    public decimal Profit { get; set; }
    public decimal Yield { get; set; }
    public decimal WinRate { get; set; }
    public int BetsCount { get; set; }
}

}
