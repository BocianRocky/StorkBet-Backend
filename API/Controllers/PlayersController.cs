using System.Security.Claims;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly IPlayerRepository _playerRepository;

    public PlayersController(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    [HttpGet("me")]
    [Authorize(Policy = "PlayerOnly")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var player = await _playerRepository.GetByIdAsync(userId);
        if (player == null) return NotFound();

        return Ok(new
        {
            name = player.Name,
            lastName = player.LastName,
            accountBalance = player.AccountBalance
        });
    }
}


