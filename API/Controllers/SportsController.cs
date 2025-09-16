namespace API.Controllers;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class SportsController:ControllerBase
{
    private readonly ISportService _sportService;
    private readonly ISportSyncService _sportSyncService;

    public SportsController(ISportService sportService, ISportSyncService sportSyncService)
    {
        _sportService = sportService;
        _sportSyncService = sportSyncService;
    }

    [HttpPost("sync")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Sync()
    {
        await _sportSyncService.SyncSportsAsync();
        return Ok("Sports synchronized successfully");
    }

    [HttpGet("all")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var sports = await _sportService.GetAllSportsAsync();
        return Ok(sports);
    }
    [HttpGet("grouped")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGroupedSports()
    {
        var groupedSports = await _sportService.GetGroupedSportsAsync();
        return Ok(groupedSports);
    }
    
}