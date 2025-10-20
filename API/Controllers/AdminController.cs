using System.Security.Claims;
using API.DTOs;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAdminStatisticsService _adminStatisticsService;
    private readonly IPlayerRepository _playerRepository;

    public AdminController(IAdminStatisticsService adminStatisticsService, IPlayerRepository playerRepository)
    {
        _adminStatisticsService = adminStatisticsService;
        _playerRepository = playerRepository;
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
        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();
        if(userId!=1) return Forbid("Brak uprawnień administratora.");
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

    /// <summary>
    /// Pobiera szczegóły wybranego gracza (statystyki i transakcje)
    /// </summary>
    [HttpGet("players-profit/{playerId:int}")]
    public async Task<ActionResult<PlayerDetailsDto>> GetPlayerDetails([FromRoute] int playerId)
    {
        try
        {
            var result = await _adminStatisticsService.GetPlayerDetailsAsync(playerId);
            if (result == null) return NotFound();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd podczas pobierania szczegółów gracza: {ex.Message}");
        }
    }
    
    [HttpGet("events/uncompleted")]
    public async Task<ActionResult<List<UncompletedEventsDto>>> GetUncompletedEvents()
    {
        try
        {
            var result = await _adminStatisticsService.GetUncompletedEventsAsync();
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd podczas pobierania niezakończonych wydarzeń: {ex.Message}");
        }
    }
    [HttpPost("update-event-result")]
    public async Task<IActionResult> UpdateEventResult([FromBody] UpdateEventResultDto dto)
    {
        try
        {
            await _adminStatisticsService.UpdateEventResultAsync(
                dto.EventId,
                dto.Team1Id,
                dto.Team2Id,
                dto.Team1Score,
                dto.Team2Score
            );
            return Ok("Wynik wydarzenia został zaktualizowany pomyślnie.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd podczas aktualizacji wyniku wydarzenia: {ex.Message}");
        }
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlayer(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        await _playerRepository.DeletePlayerAsync(id);
        
        return Ok("użytkownik usunięty");
    }
    
}
