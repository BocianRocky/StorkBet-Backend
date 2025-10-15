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
}
