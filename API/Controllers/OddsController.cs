using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OddsController: ControllerBase
{
    private readonly IOddsSyncService _oddsSyncService;
    private readonly IOddsService _oddsService;
    private readonly IScoreSyncService _scoreSyncService;
    
    public OddsController( IOddsSyncService oddsSyncService, IOddsService oddsService, IScoreSyncService scoreSyncService)
    {
        _oddsSyncService = oddsSyncService;
        _oddsService = oddsService;
        _scoreSyncService = scoreSyncService;
    }
    
    
    [HttpPost("{sportKey}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetOddsForSport(string sportKey)
    {
        await _oddsSyncService.SyncEventsAndOddsAsync(sportKey);
        return Ok(new { Message = $"Odds for {sportKey} imported" });
    }
    
    

    [HttpGet("{sportKey}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEventsWithOddsBySport(string sportKey)
    {
        var eventsWithOdds = await _oddsService.GetEventsWithOddsBySportAsync(sportKey);

        if (eventsWithOdds == null || !eventsWithOdds.Any())
            return NotFound($"Nie znaleziono eventów dla sportu '{sportKey}'.");

        return Ok(eventsWithOdds);
    }

    [HttpPost("sync-scores/{sportKey}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SyncScoresForSport(string sportKey, [FromQuery] int daysFrom = 3)
    {
        try
        {
            await _scoreSyncService.SyncScoresBySportAsync(sportKey, daysFrom);
            return Ok(new { Message = $"Scores for {sportKey} synchronized successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("sync-scores/all")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SyncScoresForAllSports([FromQuery] int daysFrom = 3)
    {
        try
        {
            await _scoreSyncService.SyncScoresForAllSportsAsync(daysFrom);
            return Ok(new { Message = "Scores for all sports synchronized successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}