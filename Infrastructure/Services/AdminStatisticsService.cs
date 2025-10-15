using Application.DTOs;
using Application.Interfaces;
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
}
