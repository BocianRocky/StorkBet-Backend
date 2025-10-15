using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAdminStatisticsService _adminStatisticsService;

    public AdminController(IAdminStatisticsService adminStatisticsService)
    {
        _adminStatisticsService = adminStatisticsService;
    }

    /// <summary>
    /// Pobiera stosunek wygranych/przegranych kuponów
    /// </summary>
    [HttpGet("win-loss-ratio")]
    public async Task<ActionResult<WinLossRatioDto>> GetWinLossRatio()
    {
        try
        {
            var result = await _adminStatisticsService.GetWinLossRatioAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd podczas pobierania statystyk wygranych/przegranych: {ex.Message}");
        }
    }

    /// <summary>
    /// Pobiera liczbę kuponów w poszczególnych miesiącach bieżącego roku
    /// </summary>
    [HttpGet("monthly-coupons")]
    public async Task<ActionResult<IEnumerable<MonthlyCouponsDto>>> GetMonthlyCoupons()
    {
        try
        {
            var result = await _adminStatisticsService.GetMonthlyCouponsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd podczas pobierania statystyk miesięcznych kuponów: {ex.Message}");
        }
    }

    /// <summary>
    /// Pobiera zysk bukmachera
    /// </summary>
    [HttpGet("bookmaker-profit")]
    public async Task<ActionResult<BookmakerProfitDto>> GetBookmakerProfit()
    {
        try
        {
            var result = await _adminStatisticsService.GetBookmakerProfitAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd podczas pobierania zysku bukmachera: {ex.Message}");
        }
    }

    /// <summary>
    /// Pobiera średnią stawkę na miesiąc
    /// </summary>
    [HttpGet("monthly-average-stake")]
    public async Task<ActionResult<IEnumerable<MonthlyAverageStakeDto>>> GetMonthlyAverageStake()
    {
        try
        {
            var result = await _adminStatisticsService.GetMonthlyAverageStakeAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd podczas pobierania średniej stawki miesięcznej: {ex.Message}");
        }
    }

    /// <summary>
    /// Pobiera liczbę kuponów na poszczególne sporty
    /// </summary>
    [HttpGet("sport-coupons")]
    public async Task<ActionResult<IEnumerable<SportCouponsDto>>> GetSportCoupons()
    {
        try
        {
            var result = await _adminStatisticsService.GetSportCouponsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd podczas pobierania statystyk kuponów na sporty: {ex.Message}");
        }
    }

    /// <summary>
    /// Pobiera skuteczność typów na poszczególne sporty
    /// </summary>
    [HttpGet("sport-effectiveness")]
    public async Task<ActionResult<IEnumerable<SportEffectivenessDto>>> GetSportEffectiveness()
    {
        try
        {
            var result = await _adminStatisticsService.GetSportEffectivenessAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd podczas pobierania skuteczności sportów: {ex.Message}");
        }
    }

    /// <summary>
    /// Pobiera wszystkie statystyki administratora w jednym wywołaniu
    /// </summary>
    [HttpGet("all-statistics")]
    public async Task<ActionResult<object>> GetAllStatistics()
    {
        try
        {
            var winLossRatioTask = _adminStatisticsService.GetWinLossRatioAsync();
            var monthlyCouponsTask = _adminStatisticsService.GetMonthlyCouponsAsync();
            var bookmakerProfitTask = _adminStatisticsService.GetBookmakerProfitAsync();
            var monthlyAverageStakeTask = _adminStatisticsService.GetMonthlyAverageStakeAsync();
            var sportCouponsTask = _adminStatisticsService.GetSportCouponsAsync();
            var sportEffectivenessTask = _adminStatisticsService.GetSportEffectivenessAsync();

            await Task.WhenAll(winLossRatioTask, monthlyCouponsTask, bookmakerProfitTask, 
                              monthlyAverageStakeTask, sportCouponsTask, sportEffectivenessTask);

            var result = new
            {
                WinLossRatio = await winLossRatioTask,
                MonthlyCoupons = await monthlyCouponsTask,
                BookmakerProfit = await bookmakerProfitTask,
                MonthlyAverageStake = await monthlyAverageStakeTask,
                SportCoupons = await sportCouponsTask,
                SportEffectiveness = await sportEffectivenessTask
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd podczas pobierania wszystkich statystyk: {ex.Message}");
        }
    }

    /// <summary>
    /// Pobiera ranking graczy po zysku
    /// </summary>
    [HttpGet("players-profit")]
    public async Task<ActionResult<IEnumerable<PlayerProfitDto>>> GetPlayersProfit()
    {
        try
        {
            var result = await _adminStatisticsService.GetPlayersProfitAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd podczas pobierania zysków graczy: {ex.Message}");
        }
    }
}
