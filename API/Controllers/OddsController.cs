using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> GetOddsForSport(string sportKey)
    {
        await _oddsSyncService.SyncEventsAndOddsAsync(sportKey);
        return Ok(new { Message = $"Odds for {sportKey} imported" });
    }
    
    [HttpGet("{sportKey}")]
    public async Task<IActionResult> GetOddsBySport(string sportKey)
    {
        var odds = await _oddsService.GetOddsBySportAsync(sportKey);

        if (odds == null || !odds.Any())
            return NotFound($"Nie znaleziono kursów dla sportu '{sportKey}'.");

        return Ok(odds);
    }
}