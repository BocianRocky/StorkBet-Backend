using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OddsController: ControllerBase
{
    private readonly IOddsSyncService _oddsSyncService;
    
    public OddsController( IOddsSyncService oddsSyncService)
    {
        _oddsSyncService = oddsSyncService;
    }
    
    
    [HttpPost("{sportKey}")]
    public async Task<IActionResult> GetOddsForSport(string sportKey)
    {
        await _oddsSyncService.SyncEventsAndOddsAsync(sportKey);
        return Ok(new { Message = $"Odds for {sportKey} imported" });
    }
}