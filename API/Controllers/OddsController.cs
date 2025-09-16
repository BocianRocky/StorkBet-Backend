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
    
    public OddsController( IOddsSyncService oddsSyncService, IOddsService oddsService)
    {
        _oddsSyncService = oddsSyncService;
        _oddsService = oddsService;
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
}