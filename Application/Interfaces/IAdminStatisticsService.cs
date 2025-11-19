using Application.DTOs;

namespace Application.Interfaces;

public interface IAdminStatisticsService
{
    Task<WinLossRatioDto> GetWinLossRatioAsync();
    Task<IEnumerable<MonthlyCouponsDto>> GetMonthlyCouponsAsync();
    Task<BookmakerProfitDto> GetBookmakerProfitAsync();
    Task<IEnumerable<MonthlyAverageStakeDto>> GetMonthlyAverageStakeAsync();
    Task<IEnumerable<SportCouponsDto>> GetSportCouponsAsync();
    Task<IEnumerable<SportEffectivenessDto>> GetSportEffectivenessAsync();
    Task<IEnumerable<PlayerProfitDto>> GetPlayersProfitAsync();
    Task<PlayerDetailsDto?> GetPlayerDetailsAsync(int playerId);
    Task<List<UncompletedEventsDto>> GetUncompletedEventsAsync();
    Task UpdateEventResultAsync(
        int eventId,
        int team1Id,
        int team2Id,
        int team1Score,
        int team2Score
    );
    Task<IEnumerable<UserRankingDto>> GetUserRankingAsync(int topCount = 30);
}
