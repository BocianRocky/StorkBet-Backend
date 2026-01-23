using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HomeController : ControllerBase
{
    private readonly IEventService _eventService;

    public HomeController(IEventService eventService)
    {
        _eventService = eventService;
    }
    
    [HttpGet("popular-events")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPopularEvents([FromQuery] int limit = 21, [FromQuery] int daysAhead = 7)
    {
        try
        {
            var popularEvents = await _eventService.GetPopularEventsAsync(limit, daysAhead);

            if (popularEvents == null || !popularEvents.Any())
                return NotFound("Nie znaleziono popularnych spotkań.");

            return Ok(popularEvents);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Błąd podczas pobierania popularnych spotkań: {ex.Message}" });
        }
    }
}

