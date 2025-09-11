namespace API.Controllers;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SportsController:ControllerBase
{
    private readonly ISportService _sportService;

    public SportsController(ISportService sportService)
    {
        _sportService = sportService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var sports = await _sportService.GetAllSportsAsync();
        return Ok(sports);
    }

    [HttpPost]
    public async Task<IActionResult> AddOrUpdate([FromBody] Sport sport)
    {
        await _sportService.AddOrUpdateSportAsync(sport);
        return Ok(sport);
    }
}